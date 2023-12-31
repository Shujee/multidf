﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using VMBase;

namespace MultiDF.VM
{
  internal class DefaultQAExtractionStrategy : IQAExtractionStrategy
  {
    enum QAExtractionState
    {
      Start,
      Question,
      Choices,
      Answer
    }

    //This RegEx will find Question Number paragraphs
    private readonly Regex RE_QNumberWithHardReturn = new Regex(@"^\f?(?<ExtraSpaceAtStart>[\t\r\n ]+)?(((?<Index>\d+)\s*(?<FinalDot>\.)?)|((?<HasQ>[Q])\s*(?<Index>\d+)\s*(?<FinalDot>\.?)))\s*[\r\n\x0B](?<Body>.*)", RegexOptions.ExplicitCapture);
    private readonly char[] TrimChars = new char[] { '\r', '\n', '\a', ' ', '\t', '\f' };

    /// <summary>
    /// Tries to extract question number from the paragraph text. Returns null if specified paragraph text does not match delimiter pattern.
    /// </summary>
    /// <param name="paragraphText"></param>
    /// <returns></returns>
    public int? ParseQuestionNumber(string paragraphText)
    {
      var M = RE_QNumberWithHardReturn.Match(paragraphText);

      if (M.Success)
        return int.Parse(M.Groups["Index"].Value);
      else
        return null;
    }

    public List<QA> ExtractQAs(List<WordParagraph> paragraphs, CancellationToken token)
    {
      if (paragraphs == null || paragraphs.Count == 0)
        return null;

      //Document being processed contains MCQs. Each MCQ starts with Question Number on a single line, followed by Question text on the next line
      //followed by multiple answers, each on a separate line. We need to group these lines (paragraphs) such that one question and all its
      //answers are in one group. We call this group (question number, body, answers) a "QA". 

      //This loop will mark the start and end of each QA. We simply iterate through the paragraphs and try to locate a paragraph that
      //marks the beginning of a new QA. This paragraph will have question number followed by a period followed by a (hard or soft) line break.
      //When we find such a paragraph, we mark it as the beginning of a new QA and continue our loop. When next QA beginning is found, we 
      //mark the end of previous QA. The process continues till the end of list.

      int i = 0;
      int ExpectedIndex = 1; //to detect delimiter typos / input mistakes
      List<QA> Result = new List<QA>();

      while (i < paragraphs.Count - 1)
      {
        var QA = ExtractNextQA(paragraphs, ref i);

        if(QA == null ||  string.IsNullOrEmpty(QA.Question))
        {
          //if we get a null before we reach the end of paragraphs list, there is a problem in the source document.
          if (i < paragraphs.Count)
          {
            var Ex = new System.Exception($"Missing question at Index {ExpectedIndex}. Import process will abort now.");
            Ex.Data.Add("Paragraph", paragraphs[i].Start);
            Ex.Data.Add("QuestionIndex", ExpectedIndex);
            throw Ex;
          }
        }
        else if (QA.Index != ExpectedIndex)
        {
          //If the extracted index is not the same as expected one, abort the process
          var Ex = new System.Exception($"Unexpected question index found at Question {QA.Index}. Expected index was {ExpectedIndex}. Import process will abort now.");
          Ex.Data.Add("Paragraph", paragraphs[i].Start);
          Ex.Data.Add("QuestionIndex", QA.Index);
          throw Ex;
        }

        if (QA != null)
          Result.Add(QA);

        ExpectedIndex++;

        if (token.IsCancellationRequested)
          return null;
      }

      return Result;
    }

    /// <summary>
    /// Extracts next Question-Answer block from the supplied list of paragraphs.
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    private QA ExtractNextQA(List<WordParagraph> paragraphs, ref int i)
    {
      QA QA = new QA();

      QAExtractionState state;

      //skip empty lines at the start till we find the Question Number paragraph
      while (i < paragraphs.Count && !RE_QNumberWithHardReturn.IsMatch(paragraphs[i].Text))
        i++;

      if (i < paragraphs.Count)
      {
        state = QAExtractionState.Start;
        QA.Start = paragraphs[i].Start;
        QA.StartPage = paragraphs[i].StartPage;
        QA.StartY = paragraphs[i].StartY;

        //if we get here, we know that we have found a question delimiter, so we'll extract question number from it.
        var Match = RE_QNumberWithHardReturn.Match(paragraphs[i].Text);

        QA.Index = int.Parse(Match.Groups["Index"].Value);

        if (state == QAExtractionState.Start)
        {
          if (!Match.Groups["HasQ"].Success && !Match.Groups["FinalDot"].Success)
          {
            var Ex = new System.Exception($"Incorrect delimiter format found at Question {QA.Index}. Question number does not end with a period.");
            Ex.Data.Add("Paragraph", paragraphs[i].Start);
            Ex.Data.Add("QuestionIndex", QA.Index);
            throw Ex;
          }
          else if (Match.Groups["ExtraSpaceAtStart"].Success)
          {
            var Ex = new System.Exception($"Incorrect delimiter format found at Question {QA.Index}. Question number is preceded by unnecessary space(s).");
            Ex.Data.Add("Paragraph", paragraphs[i].Start);
            Ex.Data.Add("QuestionIndex", QA.Index);
            throw Ex;
          }
        }

        //This handles the rare situation where source document might have a soft return (SHIFT + ENTER) immediately after the QA index. Soft returns look just like
        //normal (hard) returns, but Word doesn't treat it as the beginning of a new paragraph and thus returns the text of next (visual) paragraph concatenated 
        //after the QA index number. We extract that part in a group named "Body" using our regular expression.
        if (!string.IsNullOrEmpty(Match.Groups["Body"].Value))
        {
          QA.Question = Match.Groups["Body"].Value;
        }

        i++;

        //skip any empty lines after the question delimiter and before the question body
        while (i < paragraphs.Count && string.IsNullOrWhiteSpace(paragraphs[i].Text.Trim(TrimChars)))
          i++;

        state = QAExtractionState.Question;

        //subsequent paragraphs constitute question body, choices section and answer till we find the next paragraph that is a delimiter
        //note: sometimes choices section or question body may contain paragraphs that match the delimiter regex. To handle that situation, we distinguish genuine delimiters 
        //by looking at their Type property, which must NOT be NumberedListTableheader, Tableheader or TableRow for delimiters.
        while (i < paragraphs.Count &&
            (
            paragraphs[i].Type == ParagraphType.NumberedList ||
            paragraphs[i].Type == ParagraphType.TableHeader ||
            paragraphs[i].Type == ParagraphType.TableRow ||
            !RE_QNumberWithHardReturn.IsMatch(paragraphs[i].Text)
            )
      )
        {
          string NormalizedText = paragraphs[i].Text.ToLower().Trim(TrimChars);

          //skip empty paragraphs
          if (string.IsNullOrWhiteSpace(NormalizedText))
          {
            i++;
            continue;
          }

          switch (state)
          {
            case QAExtractionState.Question:
              if (paragraphs[i].Type == ParagraphType.NumberedList)
              {
                state = QAExtractionState.Choices;
                QA.Choices.Add(paragraphs[i].Text);
                QA.ChoicesUpper.Add(paragraphs[i].Text.ToUpperInvariant());
              }
              else if (NormalizedText.Contains("answer area:"))
              {
                state = QAExtractionState.Choices;
              }
              else
              {
                //Append to question body, but skip empty lines.
                if (!string.IsNullOrWhiteSpace(NormalizedText))
                {
                  QA.Question += paragraphs[i].Text;
                }
              }
              break;

            case QAExtractionState.Choices:
              if (paragraphs[i].Type == ParagraphType.NumberedList || paragraphs[i].Type == ParagraphType.TableRow)
              {
                QA.Choices.Add(paragraphs[i].Text);
                QA.ChoicesUpper.Add(paragraphs[i].Text.ToUpperInvariant());
              }
              else
              {
                if (paragraphs[i].Type != ParagraphType.TableHeader)
                {
                  if (NormalizedText.StartsWith("answer:"))
                  {
                    QA.Answer = paragraphs[i].Text.Trim().Remove(0, 7).Trim(TrimChars);
                    state = QAExtractionState.Answer;
                  }
                  else if (NormalizedText.StartsWith("ans:"))
                  {
                    QA.Answer = paragraphs[i].Text.Trim().Remove(0, 4).Trim(TrimChars);
                    state = QAExtractionState.Answer;
                  }
                }
              }
              break;

            case QAExtractionState.Answer:
              QA.Answer += paragraphs[i].Text;
              break;
          }

          i++;

          if (QA.Answer != null)
            break;
        }
      }

      QA.End = paragraphs[i - 1].End;
      QA.EndPage = paragraphs[i - 1].EndPage;
      QA.EndY = paragraphs[i - 1].EndY;

      if (string.IsNullOrWhiteSpace(QA.Question)) //couldn't even find the question body? return null (this can happen if there are empty lines after the last question)
        return null;
      else
        return QA;
    }

    /// <summary>
    /// Returns all paragraphs that contain QA Delimiters.
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public List<WordParagraph> ExtractDelimiterParagraphs(List<WordParagraph> paragraphs, CancellationToken token, bool throwOnSequenceError)
    {
      if (paragraphs == null || paragraphs.Count == 0)
        return null;

      //Document being processed contains MCQs. Each MCQ starts with Question Number on a single line, followed by Question text on the next line
      //followed by multiple answers, each on a separate line. We need to group these lines (paragraphs) such that one question and all its
      //answers are in one group. Here we do that.

      //This loop will mark the start and end of each QA. We simply iterate through the paragraphs and try to locate a paragraph that
      //marks the beginning of a new QA. This paragraph will have question number followed by a period followed by a (hard or soft) line break.
      //When we find such a paragraph, we mark it as the beginning of a new QA and continue our loop. When next QA beginning is found, we 
      //mark the end of previous QA. The process continues till the end of list.

      int i = 0;
      int ExpectedIndex = 1; //to detect delimiter typos / input mistakes
      List<WordParagraph> Result = new List<WordParagraph>();

      while (i < paragraphs.Count - 1)
      {
        var DelimiterParagraph = FindNextDelimiterParagraph(paragraphs, ref i);

        //finally make sure that the question index matches the expected index. If not, give user a chance to look into the source document manually.
        if (DelimiterParagraph.Value != null && DelimiterParagraph.Key != ExpectedIndex)
        {
          //Since we have merged multiple documents, we could encounter restarted numbering at any point. If this QA's index is 1 instead of previous plus one, we know
          //that the content of next document has started and we'll reset our expected index.
          if (DelimiterParagraph.Key == 1)
            ExpectedIndex = 1;
          else
          {
            if (throwOnSequenceError)
            {
              var Ex = new System.Exception($"Unexpected question index found at Question {DelimiterParagraph.Key}. Expected index was {ExpectedIndex}. Import process will abort now.");
              Ex.Data.Add("Paragraph", paragraphs[i].Start);
              Ex.Data.Add("QuestionIndex", DelimiterParagraph.Key);
              throw Ex;
            }
          }
        }

        if (DelimiterParagraph.Value != null)
          Result.Add(DelimiterParagraph.Value);

        ExpectedIndex++;

        if (token.IsCancellationRequested)
          return null;

        ViewModelLocator.Main.UpdateProgress(false, null, (((float)i + 1) / paragraphs.Count));
      }

      return Result;
    }

    public WordParagraph ExtractNearestIncorrectDelimiterParagraphs(List<WordParagraph> paragraphs, int startIndex)
    {
      if (paragraphs == null || paragraphs.Count == 0)
        return null;

      int i = startIndex;

      //Extract current paragraph's index number. We expect caller to call this function while the cursor is in QA Index number paragraph.
      KeyValuePair<int, WordParagraph> CurPara;
      try
      {
        CurPara = FindNextDelimiterParagraph(paragraphs, ref i);

        if (CurPara.Key == -1)
          return null;
      }
      catch (System.Exception)
      {
        return null;
      }

      int ExpectedIndex = CurPara.Key + 1;

      while (i < paragraphs.Count - 1)
      {
        var DelimiterParagraph = FindNextDelimiterParagraph(paragraphs, ref i);

        //finally make sure that the question index matches the expected index. If not, give user a chance to look into the source document manually.
        if (DelimiterParagraph.Value != null && DelimiterParagraph.Key != ExpectedIndex)
          return DelimiterParagraph.Value;

        ExpectedIndex++;

        ViewModelLocator.Main.UpdateProgress(false, null, (((float)i + 1) / paragraphs.Count));
      }

      return null;
    }

      /// <summary>
      /// Returns the nearest delimiter paragraph and moves i (current record pointer) to the end of that QA.
      /// </summary>
      /// <param name="paragraphs"></param>
      /// <param name="i"></param>
      /// <returns></returns>
    private KeyValuePair<int, WordParagraph> FindNextDelimiterParagraph(List<WordParagraph> paragraphs, ref int i)
    {
      WordParagraph P = null;
      int Index = -1;
      QAExtractionState state;

      //skip empty lines at the start till we find the Question Number paragraph
      while (i < paragraphs.Count && !RE_QNumberWithHardReturn.IsMatch(paragraphs[i].Text))
        i++;

      if (i < paragraphs.Count)
      {
        state = QAExtractionState.Start;
        P = paragraphs[i];

        //if we get here, we know that we have found a question delimiter, so we'll extract question number from it.
        var Match = RE_QNumberWithHardReturn.Match(paragraphs[i].Text);

        Index = int.Parse(Match.Groups["Index"].Value);

        if (state == QAExtractionState.Start)
        {
          if (!Match.Groups["HasQ"].Success && !Match.Groups["FinalDot"].Success)
          {
            var Ex = new System.Exception($"Incorrect delimiter format found at Question {Index}. Question number does not end with a period.");
            Ex.Data.Add("Paragraph", paragraphs[i].Start);
            Ex.Data.Add("QuestionIndex", Index);
            throw Ex;
          }
          else if (Match.Groups["ExtraSpaceAtStart"].Success)
          {
            var Ex = new System.Exception($"Incorrect delimiter format found at Question {Index}. Question number is preceded by unnecessary space(s).");
            Ex.Data.Add("Paragraph", paragraphs[i].Start);
            Ex.Data.Add("QuestionIndex", Index);
            throw Ex;
          }
        }

        i++;

        //skip any empty lines after the question delimiter and before the question body
        while (i < paragraphs.Count && string.IsNullOrWhiteSpace(paragraphs[i].Text.Trim(TrimChars)))
          i++;

        state = QAExtractionState.Question;

        //subsequent paragraphs constitute question body, choices section and answer till we find the next paragraph that is a delimiter
        //note: sometimes choices section or question body may contain paragraphs that match the delimiter regex. To handle that situation, we distinguish genuine delimiters 
        //by looking at their Type property, which for delimiters must NOT be NumberedListTableheader, Tableheader or TableRow.
        while (i < paragraphs.Count &&
            (
            paragraphs[i].Type == ParagraphType.NumberedList ||
            paragraphs[i].Type == ParagraphType.TableHeader ||
            paragraphs[i].Type == ParagraphType.TableRow ||
            !RE_QNumberWithHardReturn.IsMatch(paragraphs[i].Text)
            ))
        {
          string NormalizedText = paragraphs[i].Text.ToLower().Trim(TrimChars);

          //skip empty paragraphs
          if (string.IsNullOrWhiteSpace(NormalizedText))
          {
            i++;
            continue;
          }

          bool AnswerFound = false;
          switch (state)
          {
            case QAExtractionState.Question:
              if (paragraphs[i].Type == ParagraphType.NumberedList)
              {
                state = QAExtractionState.Choices;
              }
              else if (NormalizedText.Contains("answer area:"))
              {
                state = QAExtractionState.Choices;
              }
              break;

            case QAExtractionState.Choices:
              if (paragraphs[i].Type != ParagraphType.NumberedList && paragraphs[i].Type != ParagraphType.TableRow)
              {
                if (paragraphs[i].Type != ParagraphType.TableHeader)
                {
                  if (NormalizedText.StartsWith("answer:"))
                    state = QAExtractionState.Answer;
                  else if (NormalizedText.StartsWith("ans:"))
                    state = QAExtractionState.Answer;
                }
              }
              break;

            case QAExtractionState.Answer:
              AnswerFound = true;
              break;
          }

          i++;

          if (AnswerFound)
            break;
        }
      }


      if (i <= paragraphs.Count && P != null && RE_QNumberWithHardReturn.IsMatch(P.Text))
        return new KeyValuePair<int, WordParagraph>(Index, P);
      else
        return new KeyValuePair<int, WordParagraph>(-1, null);
    }
  }
}
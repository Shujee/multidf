using System.Collections.Generic;
using System.Threading;

namespace DuplicateFinderMulti.VM
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
    System.Text.RegularExpressions.Regex RE_QNumberWithHardReturn = new System.Text.RegularExpressions.Regex(@"^(((?<Index>\d+)\.)|([Q](?<Index>\d+)\.?))\s*[\r\n\x0B]", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
                                                                                                             

    public List<QA> Extract(List<WordParagraph> paragraphs, CancellationToken tok)
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
      List<QA> Result = new List<QA>();

      while (i < paragraphs.Count - 1)
      {
        var QA = ExtractQA(paragraphs, ref i);
        Result.Add(QA);

        tok.ThrowIfCancellationRequested();
      }

      return Result;
    }

    /// <summary>
    /// Extracts next Question-Answer block from the supplied list of paragraphs.
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    private QA ExtractQA(List<WordParagraph> paragraphs, ref int i)
    {
      QA QA = new QA();

      QAExtractionState state = QAExtractionState.Start;

      //skip empty lines at the start till we find the Question Number paragraph
      while (i < paragraphs.Count && !RE_QNumberWithHardReturn.IsMatch(paragraphs[i].Text))
        i++;

      if (i < paragraphs.Count)
      {
        state = QAExtractionState.Question;
        QA.Start = paragraphs[i].Start;

        //if we get here, we know that we have found a question delimiter, so we'll extract question number from it.
        var Match = RE_QNumberWithHardReturn.Match(paragraphs[i].Text);
        QA.Index = int.Parse(Match.Groups["Index"].Value);
        i++;

        //subsequent paragraphs constitute question body till we find the next paragraph that uses list style
        while (i < paragraphs.Count && !RE_QNumberWithHardReturn.IsMatch(paragraphs[i].Text))
        {
          switch (state)
          {
            case QAExtractionState.Question:
              if (paragraphs[i].IsSimpleNumberingListStyle)
              {
                state = QAExtractionState.Choices;
                QA.Choices.Add(paragraphs[i].Text);
              }
              else
              {
                QA.Question += paragraphs[i];
              }
              break;

            case QAExtractionState.Choices:
              if (paragraphs[i].IsSimpleNumberingListStyle)
                QA.Choices.Add(paragraphs[i].Text);
              else
                state = QAExtractionState.Answer;
              break;

            case QAExtractionState.Answer:
              string NormalizedText = paragraphs[i].Text.ToLower().Trim();
              if (NormalizedText.StartsWith("answer:"))
                QA.Answer = NormalizedText.Remove(0, 7).Trim();
              else if (NormalizedText.StartsWith("ans:"))
                QA.Answer = NormalizedText.Remove(0, 4).Trim();

              break;
          }

          i++;

          if (QA.Answer != null)
            break;
        }
      }

      QA.End = paragraphs[i - 1].End;

      return QA;
    }
  }
}
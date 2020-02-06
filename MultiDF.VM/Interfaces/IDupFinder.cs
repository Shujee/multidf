using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiDF.VM
{
  public interface IDupFinder
  {
    List<WordParagraph[]> Find(List<WordParagraph> paras, int maxDistance, bool ignoreCase, int? minParaLength, Action<int, int, string, bool> updateStatusLabel, CancellationToken tok);
  }
}
﻿using System;
using System.Linq;
using System.Threading;
using DuplicateFinderMulti.VM;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DuplicateFinderMulti.Test
{
  [TestClass]
  public class TestBase
  {
    protected readonly CancellationTokenSource _TokenSource = new CancellationTokenSource();
    protected CancellationToken token;

    public TestBase()
    {
      token = _TokenSource.Token;
      SimpleIoc.Default.Register<IWordService, TestWordService>();
      GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
    }

    protected Project CreateRandomProject()
    {
      var P = new Project()
      {
        IsDirty = false,
        Name = "Project 1",
        SavePath = @"F:\some\path\project1.xml"
      };

      P.AllXMLDocs = new System.Collections.ObjectModel.ObservableCollection<XMLDoc>(Enumerable.Range(1, 4).Select(i => CreateRandomXMLDoc()));

      return P;
    }

    protected XMLDoc CreateRandomXMLDoc()
    {
      var Doc = new XMLDoc
      {
        LastModified = DateTime.Now.AddDays(Faker.RandomNumber.Next(-100, 100)),
        SourcePath = @"F:\Some\Path\" + Faker.Name.FullName(Faker.NameFormats.Standard) + ".docx",
        Size = Faker.RandomNumber.Next(10000, 1000000)
      };

      Doc.QAs = Enumerable.Range(1, Faker.RandomNumber.Next(10, 60)).Select(i => CreateRandomQA(Doc, i)).ToList();

      return Doc;
    }

    protected QA CreateRandomQA(XMLDoc parent, int index)
    {
      var QA = new QA
      {
        Answer = Faker.Lorem.GetFirstWord(),
        Choices = Enumerable.Range(1, 6).Select(i => Faker.Lorem.Sentence(Faker.RandomNumber.Next(1, 6))).ToList(),
        Doc = parent,
        Start = Faker.RandomNumber.Next(1, 1000),
        Index = index,
        Question = Faker.Lorem.Sentence(50)
      };

      QA.End = QA.Start + Faker.RandomNumber.Next(100, 500);

      return QA;
    }
  }
}

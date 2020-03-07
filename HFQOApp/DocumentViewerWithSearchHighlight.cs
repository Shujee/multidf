//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Reflection;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Media;

namespace HFQOApp
{
  /// <summary>
  /// DocumentViewer that has its Search Box overridden in order to select multiple results in the document.
  /// Use IsMultiSearchEnabled to turn off this behavior.
  /// The number of results can be limited with the MaxSearchResults property.
  /// </summary>
  //class DocumentViewerWithSearchHighlight : DocumentViewer
  //{
  //  private ToolBar _myfindToolbar; // MS.Internal.Documents.FindToolBar
  //  private object _mydocumentScrollInfo; // MS.Internal.Documents.DocumentGrid

  //  private MethodInfo _miFind; // DocumentViewerBase.Find(FindToolBar)
  //  private MethodInfo _miGoToTextBox; // FindToolBar.GoToTextBox()
  //  private MethodInfo _miMakeSelectionVisible; // DocumentGrid.MakeSelectionVisible()

  //  /// <summary>
  //  /// Limit for returned search results. 0 for no limit, default is int.MaxValue.
  //  /// </summary>
  //  public int MaxSearchResults { get { return (int)GetValue(MaxSearchResultsProperty); } set { SetValue(MaxSearchResultsProperty, value); } }
  //  public static readonly DependencyProperty MaxSearchResultsProperty =
  //      DependencyProperty.Register("MaxSearchResults", typeof(int), typeof(DocumentViewerWithSearchHighlight), new PropertyMetadata(int.MaxValue));


  //  /// <summary>
  //  /// Determines if the search of the find toolbox is overridden and multiple search results are selected in the document.
  //  /// </summary>
  //  public bool IsMultiSearchEnabled { get { return (bool)GetValue(IsMultiSearchEnabledProperty); } set { SetValue(IsMultiSearchEnabledProperty, value); } }
  //  public static readonly DependencyProperty IsMultiSearchEnabledProperty =
  //      DependencyProperty.Register("IsMultiSearchEnabled", typeof(bool), typeof(DocumentViewerWithSearchHighlight), new PropertyMetadata(true));

  //  private TextBox _SearchTextBox;

  //  public override void OnApplyTemplate()
  //  {
  //    base.OnApplyTemplate();

  //    if (IsMultiSearchEnabled)
  //    {
  //      // get some private fields from the base class DocumentViewer
  //      _myfindToolbar = this.GetType().GetPrivateFieldOfBase("_findToolbar").GetValue(this) as ToolBar;
  //      _mydocumentScrollInfo = this.GetType().GetPrivateFieldOfBase("_documentScrollInfo").GetValue(this);

  //      // replace button click handler of find toolbar
  //      EventInfo evt = _myfindToolbar.GetType().GetEvent("FindClicked");
  //      ReflectionHelper.RemoveEventHandler(_myfindToolbar, evt.Name); // remove existing handler
  //      evt.AddEventHandler(_myfindToolbar, new EventHandler(OnFindInvoked)); // attach own handler

  //      // get some methods that will need to be invoked
  //      _miFind = this.GetType().GetMethod("Find", BindingFlags.NonPublic | BindingFlags.Instance);
  //      _miGoToTextBox = _myfindToolbar.GetType().GetMethod("GoToTextBox");
  //      _miMakeSelectionVisible = _mydocumentScrollInfo.GetType().GetMethod("MakeSelectionVisible");
  //    }
  //  }

  //  public void SearchText(string text)
  //  {
  //    //var SearchTextBox = FindChild<TextBox>(this, "FindTextBox");
  //    //var FindNextButton = FindChild<Button>(this, "FindNextButton");

  //    //SearchTextBox.Text = text;
  //    //FindNextButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

  //    List<Glyphs> lst = new List<Glyphs>();
  //    FindVisualChild<Glyphs>(this, lst);

  //    foreach (var g in lst)
  //    {
  //      if (g.UnicodeString.ToLower().Contains(text.ToLower()))
  //        g.Fill = Brushes.Red;
  //      else
  //        g.Fill = Brushes.Black;
  //    }
  //  }

  //  /// <summary>
  //  /// Finds a Child of a given item in the visual tree. 
  //  /// </summary>
  //  /// <param name="parent">A direct parent of the queried item.</param>
  //  /// <typeparam name="T">The type of the queried item.</typeparam>
  //  /// <param name="childName">x:Name or Name of child. </param>
  //  /// <returns>The first parent item that matches the submitted type parameter. 
  //  /// If not matching item can be found, 
  //  /// a null parent is being returned.</returns>
  //  public static T FindChild<T>(DependencyObject parent, string childName)
  //     where T : DependencyObject
  //  {
  //    // Confirm parent and childName are valid. 
  //    if (parent == null) return null;

  //    T foundChild = null;

  //    int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
  //    for (int i = 0; i < childrenCount; i++)
  //    {
  //      var child = VisualTreeHelper.GetChild(parent, i);
  //      // If the child is not of the request child type child
  //      T childType = child as T;
  //      if (childType == null)
  //      {
  //        // recursively drill down the tree
  //        foundChild = FindChild<T>(child, childName);

  //        // If the child is found, break so we do not overwrite the found child. 
  //        if (foundChild != null) break;
  //      }
  //      else if (!string.IsNullOrEmpty(childName))
  //      {
  //        var frameworkElement = child as FrameworkElement;
  //        // If the child's name is set for search
  //        if (frameworkElement != null && frameworkElement.Name == childName)
  //        {
  //          // if the child's name is of the request name
  //          foundChild = (T)child;
  //          break;
  //        }
  //      }
  //      else
  //      {
  //        // child element found.
  //        foundChild = (T)child;
  //        break;
  //      }
  //    }

  //    return foundChild;
  //  }

  //  public static void FindVisualChild<T>(DependencyObject obj, List<T> lst) where T : DependencyObject
  //  {
  //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
  //    {
  //      DependencyObject child = VisualTreeHelper.GetChild(obj, i);

  //      if (child != null && child is T t)
  //        lst.Add(t);

  //      FindVisualChild<T>(child, lst);
  //    }
  //  }

  //  /// <summary>
  //  /// This is replacing DocumentViewer.OnFindInvoked(object sender, EventArgs e)
  //  /// </summary>
  //  private void OnFindInvoked(object sender, EventArgs e)
  //  {
  //    IList allSegments = null; // collection of text segments
  //    TextRange findResult = null; // could also use object, does not need type

  //    //Give ourselves focus, this ensures that the selection
  //    //will be made visible after it's made.
  //    this.Focus();

  //    // Drill down to the list of selected text segments: DocumentViewer.TextEditor.Selection.TextSegments
  //    object textEditor = this.GetType().GetProperty("TextEditor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this); // System.Windows.Documents.TextEditor
  //    object selection = textEditor.GetType().GetProperty("Selection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(textEditor); // System.Windows.Documents.TextSelection
  //    FieldInfo fiTextSegments = selection.GetType().GetPrivateFieldOfBase("_textSegments");
  //    IList textSegments = fiTextSegments.GetValue(selection) as IList; // List<System.Windows.Documents.TextSegment>

  //    // Clearing the selection in order to start search from the beginning of the document. I suspect there might be a better way of doing this.
  //    object segmentStart = textSegments[0].GetType().GetField("_start", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(textSegments[0]); // get segment start (one textsegment is always present)
  //    int currentOffset = (int)segmentStart.GetType().GetProperty("System.Windows.Documents.ITextPointer.Offset", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(segmentStart); // get offset of segment start
  //    segmentStart = segmentStart.GetType().GetMethod("CreatePointer", new Type[] { segmentStart.GetType(), typeof(int) }).Invoke(segmentStart, new object[] { segmentStart, -currentOffset }); // set the offset back to 0

  //    textSegments[0] = textSegments[0].GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { segmentStart.GetType(), segmentStart.GetType() }, null)
  //                                               .Invoke(new object[] { segmentStart, segmentStart }); // create a new textsegment with resetted offset

  //    for (int i = 1; i < textSegments.Count; i++)
  //    {
  //      textSegments.RemoveAt(i); // remove all other segments
  //    }

  //    // Always search down
  //    _myfindToolbar.GetType().GetProperty("SearchUp").SetValue(_myfindToolbar, false);

  //    // Search and collect the find results
  //    int resultCount = 0;
  //    do
  //    {
  //      // invoke: DocumentViewerBase.Find(findToolBar)
  //      findResult = _miFind.Invoke(this, new object[] { _myfindToolbar }) as TextRange;

  //      if (findResult != null)
  //      {
  //        // get the selected TextSegments of the search
  //        textSegments = fiTextSegments.GetValue(selection) as IList; // List<System.Windows.Documents.TextSegment>
  //        if (allSegments == null)
  //          allSegments = textSegments; // first search find, set whole collection
  //        else
  //          allSegments.Add(textSegments[0]); // after first find, add to collection

  //        resultCount++;
  //      }
  //    }
  //    while (findResult != null && (MaxSearchResults == 0 || resultCount < MaxSearchResults)); // stop if no more results were found or limit is exceeded

  //    if (allSegments == null)
  //    {
  //      // alert the user that we did not find anything
  //      string searchText = _myfindToolbar.GetType().GetProperty("SearchText").GetValue(_myfindToolbar) as string;
  //      string messageString = string.Format("Searched the document. Cannot find '{0}'.", searchText);

  //      MessageBox.Show(messageString, "Find", MessageBoxButton.OK, MessageBoxImage.Asterisk);
  //    }
  //    else
  //    {
  //      // set the textsegments field to the collected search results
  //      fiTextSegments.SetValue(selection, allSegments);

  //      // this marks the text. invoke: DocumentGrid.MakeSelectionVisible()
  //      _miMakeSelectionVisible.Invoke(_mydocumentScrollInfo, null);
  //    }

  //    // put the focus back on the findtoolbar textbox to search again. invoke: FindToolBar.GoToTextBox()
  //    _miGoToTextBox.Invoke(_myfindToolbar, null);
  //  }
  //}

  //public static class ReflectionExtensions
  //{
  //  /// <summary>
  //  /// Gets private field of base class. Normally, they are not directly accessible in a GetField call.
  //  /// </summary>
  //  public static FieldInfo GetPrivateFieldOfBase(this Type type, string fieldName)
  //  {
  //    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

  //    // Declare variables
  //    FieldInfo fieldInfo = null;

  //    // Search as long as there is a type
  //    while (type != null)
  //    {
  //      // Use reflection
  //      fieldInfo = type.GetField(fieldName, bindingFlags);

  //      // Yes, do we have a field?
  //      if (fieldInfo != null) break;

  //      // Get base class
  //      type = type.BaseType;
  //    }

  //    // Return result
  //    return fieldInfo;
  //  }
  //}

  ///// <summary>
  ///// http://www.codeproject.com/Articles/103542/Removing-Event-Handlers-using-Reflection
  ///// </summary>
  //public static class ReflectionHelper
  //{
  //  static Dictionary<Type, List<FieldInfo>> dicEventFieldInfos = new Dictionary<Type, List<FieldInfo>>();

  //  static BindingFlags AllBindings
  //  {
  //    get { return BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static; }
  //  }

  //  static List<FieldInfo> GetTypeEventFields(Type t)
  //  {
  //    if (dicEventFieldInfos.ContainsKey(t))
  //      return dicEventFieldInfos[t];

  //    List<FieldInfo> lst = new List<FieldInfo>();
  //    BuildEventFields(t, lst);
  //    dicEventFieldInfos.Add(t, lst);
  //    return lst;
  //  }

  //  static void BuildEventFields(Type t, List<FieldInfo> lst)
  //  {
  //    //BindingFlags.FlattenHierarchy only works on protected & public, doesn't work because fields are private
  //    // Uses .GetEvents and then uses .DeclaringType to get the correct ancestor type so that we can get the FieldInfo.
  //    foreach (EventInfo ei in t.GetEvents(AllBindings))
  //    {
  //      Type dt = ei.DeclaringType;
  //      FieldInfo fi = dt.GetField(ei.Name, AllBindings);
  //      if (fi != null)
  //        lst.Add(fi);
  //    }
  //  }

  //  static EventHandlerList GetStaticEventHandlerList(Type t, object obj)
  //  {
  //    MethodInfo mi = t.GetMethod("get_Events", AllBindings);
  //    return (EventHandlerList)mi.Invoke(obj, new object[] { });
  //  }

  //  public static void RemoveAllEventHandlers(object obj) { RemoveEventHandler(obj, ""); }

  //  public static void RemoveEventHandler(object obj, string EventName)
  //  {
  //    if (obj == null)
  //      return;

  //    Type t = obj.GetType();
  //    List<FieldInfo> event_fields = GetTypeEventFields(t);
  //    EventHandlerList static_event_handlers = null;

  //    foreach (FieldInfo fi in event_fields)
  //    {
  //      if (EventName != "" && string.Compare(EventName, fi.Name, true) != 0)
  //        continue;

  //      // STATIC Events have to be treated differently from INSTANCE Events...
  //      if (fi.IsStatic)
  //      {
  //        if (static_event_handlers == null)
  //          static_event_handlers = GetStaticEventHandlerList(t, obj);

  //        object idx = fi.GetValue(obj);
  //        Delegate eh = static_event_handlers[idx];
  //        if (eh == null)
  //          continue;

  //        Delegate[] dels = eh.GetInvocationList();
  //        if (dels == null)
  //          continue;

  //        EventInfo ei = t.GetEvent(fi.Name, AllBindings);
  //        foreach (Delegate del in dels)
  //          ei.RemoveEventHandler(obj, del);
  //      }
  //      else
  //      {
  //        EventInfo ei = t.GetEvent(fi.Name, AllBindings);
  //        if (ei != null)
  //        {
  //          object val = fi.GetValue(obj);
  //          Delegate mdel = (val as Delegate);
  //          if (mdel != null)
  //          {
  //            foreach (Delegate del in mdel.GetInvocationList())
  //              ei.RemoveEventHandler(obj, del);
  //          }
  //        }
  //      }
  //    }
  //  }
  //}
}
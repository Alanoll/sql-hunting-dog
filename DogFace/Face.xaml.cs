﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DatabaseObjectSearcher;
using DatabaseObjectSearcherUI;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace
{
    /// <summary>
    /// Interaction logic for Face.xaml
    /// </summary>
    public partial class Face : UserControl
    {
        // these keys are used to save/load user preferences
        public const string UserPref_LastSearchText = "Last Search Text";
        public const string UserPref_ServerDatabase= "[database]:";
        public const string UserPref_LastSelectedServer = "Last Selected Server";

        BackgroundProcessor _processor = new BackgroundProcessor();

        static IStudioController _studio;
        public IStudioController StudioController
        {
            get
            {
                if (_studio == null)
                {
                    _studio = HuntingDog.DogEngine.StudioController.Current;
                }

                return _studio;
            }
            set
            {
                _studio = value;
            }
        }

        enum ERquestType:int
        {
            Server,
            Search ,
            Details
        }

        public Face()
        {
            InitializeComponent();
        }

        UserPreferencesStorage _userPref;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            var scroll = FindChild<ScrollContentPresenter>(itemsControl);
            scroll.SizeChanged += new SizeChangedEventHandler(scroll_SizeChanged);

            _processor.RequestFailed += new Action<BackgroundProcessor.Request, Exception>(_processor_RequestFailed);
            StudioController.Initialise();
            StudioController.OnServersChanged += new Action(StudioController_OnServersChanged);
            ReloadServers();

            _userPref = UserPreferencesStorage.Load();
            string lastSrvName = _userPref.GetByName(UserPref_LastSelectedServer);
            string lastSearch = _userPref.GetByName(UserPref_LastSearchText);

            if(string.IsNullOrEmpty(lastSearch))
                  txtSearch.Text = "HAHAHAH";
            else
                txtSearch.Text = lastSearch;

            //cbServer.SelectedValue = lastSrvName;
            //cbServer.SelectedItem = 

            // select first server
            if (cbServer.SelectedIndex==-1 && cbServer.Items.Count>0)
            {
                cbServer.SelectedIndex = 0;
            }    
          
        }

     

        void StudioController_OnServersChanged()
        {
            ReloadServers();

            if (cbServer.SelectedIndex == -1 && cbServer.Items.Count > 0)
                cbServer.SelectedIndex = 0;
        }

        void _processor_RequestFailed(BackgroundProcessor.Request arg1, Exception arg2)
        {
            // notify user about an error

        }

        public void ReloadServers()
        {   
            var servers = StudioController.ListServers();
            cbServer.ItemsSource = ItemFactory.BuildServer(servers);

            _processor.AddRequest(Async_ReloadServers, servers, (int)ERquestType.Server,true);
        }

        private void SetStatus(string text)
        {
            InvokeInUI(delegate { txtStatusTest.Text = text; });
        }

        // Reload all servers ad read all business object for fast search and access
        private void Async_ReloadServers(object arg)
        {
            foreach (var server in (List<string>)arg)
            {
                SetStatus("Refreshing " + server + "...");
                StudioController.RefreshServer(server);

                SetStatus("Completed " + server);
            }
        }


        
        private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = cbServer.SelectedItem as Item;
            if (sel != null)
            {
                cbDatabase.ItemsSource = ItemFactory.BuildDatabase(StudioController.ListDatabase(sel.Name));

                _databaseChangedByUser = false;
                // changed server - try to restore database user worked with last time
                var databaseName = _userPref.GetByName(UserPref_ServerDatabase + sel.Name);
                //cbDatabase.SelectedValue= databaseName;

                _databaseChangedByUser = true;

                // if we failed to select database (for example it deos not exsit any more - select first one...)
                if(cbDatabase.SelectedIndex == -1 && cbDatabase.Items.Count>0)
                {
                    cbDatabase.SelectedIndex = 0;
                }

                _userPref.StoreByName(UserPref_LastSelectedServer, sel.Name);

                cbDatabase.Focus();
                cbDatabase.IsDropDownOpen = true;
            }

            // keep track of last selected database on this server - need to restore it back!
            //DoSearch();
        }

        bool _databaseChangedByUser = true;

        string SelectedServer
        {
            get{
                if(cbServer.SelectedItem==null)
                    return null;

                return (cbServer.SelectedItem as Item).Name;
            }
        }


        string SelectedDatabase
        {
            get
            {
                if (cbDatabase.SelectedItem == null)
                    return null;

                return (cbDatabase.SelectedItem as Item).Name;
            }
        }

        private void cbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (_databaseChangedByUser)
            {
                if (SelectedServer != null && SelectedDatabase != null)
                {
                    _userPref.StoreByName(UserPref_ServerDatabase + SelectedServer,
                        SelectedDatabase);

                    txtSearch.Focus();  

                    DoSearch();
                }
            }


        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoSearch();
        }

        void DoSearch()
        {
            if (!string.IsNullOrEmpty(txtSearch.Text) && SelectedServer!=null && SelectedDatabase!=null)
            {
                var sp = new SearchAsyncParam();
                sp.Srv = SelectedServer;
                sp.Text = txtSearch.Text;
                sp.Database = SelectedDatabase;
                _processor.AddRequest(Async_PerformSearch, sp, (int)ReqType.Search, true);

                _userPref.StoreByName(UserPref_LastSearchText, txtSearch.Text);
            
            }
            else
            {
                itemsControl.ItemsSource = null;
            }
        }


        private void Async_PerformSearch(object arg)
        {
            if (arg == null)
                return;

            var par = (SearchAsyncParam)arg;
            SetStatus("Searching '" + par.Text + "' in " + par.Database);

            var result = StudioController.Find(par.Srv, par.Database, par.Text);


            InvokeInUI(delegate { 
                
                itemsControl.ItemsSource = ItemFactory.BuildFromEntries(result);
            
                itemsControl.SelectedIndex = -1;
                itemsControl.ScrollIntoView(itemsControl.SelectedItem);
            });

            SetStatus("Found " + result.Count + " objects ");

   
         }

        // small hint - to use anonomys delegates in InvokeUI method
        public delegate void AnyInvoker();
        private void InvokeInUI(AnyInvoker invoker)
        {
            Dispatcher.Invoke((Delegate)invoker);
        }

   

        private void itemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void itemsControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Stop()
        {
            _userPref.Save();
            if (_processor != null)
            {
                _processor.Stop();
                _processor = null;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

            Stop();
        }

        void InvokeDefaultOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.SelectFromTable(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsProcedure)
            {
                StudioController.ModifyProcedure(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsFunction)
            {
                StudioController.ModifyFunction(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsView)
            {
                StudioController.SelectFromView(SelectedServer, item.Entity);
            }
        }

        void InvokeActionOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.DesignTable(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsProcedure)
            {
                StudioController.ExecuteProcedure(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsFunction)
            {
                StudioController.ExecuteFunction(SelectedServer, item.Entity);
            }


        }

        void InvokeNavigationOnItem(Item item)
        {
            StudioController.NavigateObject(SelectedServer, item.Entity);
        }


        private void btnNavigationClick(object sender, RoutedEventArgs e)
        {
            var item = (Item)((Control)sender).Tag;
            InvokeNavigationOnItem(item);
  
        }

        private void btnActionClick(object sender, RoutedEventArgs e)
        {
             var item = (Item)((Control)sender).Tag;
            InvokeActionOnItem(item);
  
        }

        private void DefaultAction_Click(object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeDefaultOnItem(item);
        }
 

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
 
        }


        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Down)
            {
                if (itemsControl.Items.Count > 0)
                {

                    // move focus to result list view
                    //txtSearch.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                    MoveFocusItemsControl(false);
                    
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                txtSearch.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                e.Handled = true;
            }
        }


        private void itemsControl_KeyDown(object sender, KeyEventArgs e)
        { 
        }



        private bool MoveFocusItemsControl(bool isLast)
        {
            if (itemsControl.Items.Count > 0)
            {
                var index = isLast ? itemsControl.Items.Count - 1 : 0;
                itemsControl.SelectedIndex = index;
                var it = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as Control;
                if (it != null)
                {
                    return it.Focus();
                }
                else
                {
                    
                    itemsControl.ScrollIntoView(itemsControl.Items[index]);
                    var it1 = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as Control;
                    if (it1 != null)
                    {
                        return it1.Focus();

                    }
                }

            }

            return false;
        }

        private void itemsControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                itemsControl.SelectedIndex = -1;
                            
                // move focus to result text
                itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
              
                e.Handled = true;
            }
            else if (e.Key == Key.Up && itemsControl.SelectedIndex == 0)
            {
                itemsControl.SelectedIndex = -1;

                // jump to text search box from Result View - 
                itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
              
                e.Handled = true;
            }
            else if (e.Key == Key.Down && itemsControl.SelectedIndex == itemsControl.Items.Count-1)
            {
                // last item - do nothing
                e.Handled = true;
            }
            else if ((e.Key == Key.Enter || e.Key == Key.Space) && itemsControl.SelectedIndex != -1)
            {
                InvokeDefaultOnItem(itemsControl.SelectedItem as Item);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                MoveFocusItemsControl(true);
                e.Handled = true;

            }
            else if (e.Key == Key.Left)
            {
                MoveFocusItemsControl(false);
                e.Handled = true;
                    
            }
        }

        private void cbDatabase_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Space) && cbDatabase.IsDropDownOpen == false)
            {
                cbDatabase.IsDropDownOpen = true;
                e.Handled = true;
            }

            if (( e.Key == Key.Down || e.Key == Key.Right) && cbDatabase.IsDropDownOpen == false)
            {
                cbDatabase.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if (( e.Key == Key.Up || e.Key == Key.Left) && cbDatabase.IsDropDownOpen == false)
            {
                cbDatabase.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                e.Handled = true;
            }

            if (e.Key == Key.Space && cbDatabase.IsDropDownOpen)
            {
                foreach (var item in cbDatabase.Items)
                {
                    var it = cbDatabase.ItemContainerGenerator.ContainerFromItem(item);
                    if ((it as ComboBoxItem).IsHighlighted)
                    {
                        cbDatabase.SelectedItem = item;
                        cbDatabase.IsDropDownOpen = false;
                    }
                }
            }
        }

        private void cbServer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Space) && !cbServer.IsDropDownOpen)
            {
                cbServer.IsDropDownOpen = true;
                e.Handled = true;
            }

            if ((e.Key == Key.Down ) && !cbServer.IsDropDownOpen)
            {
                cbServer.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Down));
                e.Handled = true;
            }

            if ( e.Key == Key.Right && !cbServer.IsDropDownOpen)
            {
                cbServer.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if ((e.Key == Key.Up || e.Key == Key.Left) && !cbServer.IsDropDownOpen )
            {         
                e.Handled = true;
            }

            if (e.Key == Key.Space && cbServer.IsDropDownOpen)
            {
                foreach (var item in cbServer.Items)
                {
                    var it = cbServer.ItemContainerGenerator.ContainerFromItem(item);
                    if ((it as ComboBoxItem).IsHighlighted)
                    {
                        cbServer.SelectedItem = item;
                        cbServer.IsDropDownOpen = false;
                    }
                }              
            }
        }

        public long LastTicks = 0;

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now.Ticks - LastTicks) < 3000000)
            {
                if (itemsControl.SelectedItem != null)
                    InvokeDefaultOnItem(itemsControl.SelectedItem as Item);

            }
            LastTicks = DateTime.Now.Ticks;
        }

        void scroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sp = FindChild<VirtualizingStackPanel>(itemsControl);
            
            return;

            var gv = itemsControl.View as GridView;

            var scroll = sender as ScrollContentPresenter;

            var totalWidth = scroll.ActualWidth;
            
            totalWidth -= gv.Columns[0].ActualWidth;
            totalWidth -= gv.Columns[2].ActualWidth;

            // Magic number - we need to take into acctound padding/margins and all other stuff and caclulate Width of the central column
            // we Width is too high scrool bar will appear. 
            totalWidth -= 8;

            if(totalWidth < 100)
                totalWidth  = 100;
            gv.Columns[1].Width = totalWidth;

            //var sp = FindChild<VirtualizingStackPanel>(itemsControl);
            //sp.Arrange();
            //var parent = VisualTreeHelper.GetParent(sp) as ItemsPresenter;

            //sp.Width = parent.ActualWidth;
        }

        private void itemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

      


        public static T FindChild<T>(DependencyObject from) where T : class
        {
            if (from == null)
            {
                return null;
            }

            T candidate = from as T;
            if (candidate != null)
            {
                return candidate;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(from); i++)
            {
                var isOur = FindChild<T> ( VisualTreeHelper.GetChild(from, i));
                if (isOur != null)
                    return isOur;
            }

            return null;
        }

        public static T FindAncestor<T>(DependencyObject from) where T : class
        {
            if (from == null)
            {
                return null;
            }

            T candidate = from as T;
            if (candidate != null)
            {
                return candidate;
            }

            return FindAncestor<T>(VisualTreeHelper.GetParent(from));
        }

        Brush _borderBrush = new SolidColorBrush(Color.FromRgb(0x64,0x95,0xed));
        Brush _blurBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x64, 0x95, 0xed));

       
        private void cbDatabase_GotFocus(object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = _borderBrush;
            cbDatabase.BorderBrush = _borderBrush;
        }

        private void cbDatabase_LostFocus(object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = Brushes.Transparent;
            //cbDatabase.BorderBrush = _blurBrush;
        }

        private void cbServer_GotFocus(object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = _borderBrush;
            //cbServer.BorderBrush = _borderBrush;
        }

        private void cbServer_LostFocus(object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = Brushes.Transparent;
            //cbServer.BorderBrush = _blurBrush;
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            borderText.BorderBrush = _borderBrush;
            txtSearch.SelectAll();
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            borderText.BorderBrush = _blurBrush;
        }

        private void borderText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.Focus();
        }


     

        //private void itemsControl_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    borderItems.BorderBrush = _borderBrush;
        //}

        //private void itemsControl_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    borderItems.BorderBrush = Brushes.Transparent;
        //}
    }

    class SearchAsyncParam
    {
      
        public string Srv { get; set; }
        public string Text { get; set; }
        public string Database{get;set;}
    }

    public class WidthConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var desiredWidth =  ((double)value - 8);
            desiredWidth -= 30;
            desiredWidth -= 80;

            if (desiredWidth < 100)
                desiredWidth = 100;

            return desiredWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
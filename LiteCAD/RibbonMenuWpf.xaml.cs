using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiteCAD
{
    /// <summary>
    /// Interaction logic for RibbonMenuWpf.xaml
    /// </summary>
    public partial class RibbonMenuWpf : UserControl
    {
        public RibbonMenuWpf()
        {
            InitializeComponent();

            RibbonWin.Loaded += RibbonWin_Loaded;            
        }

        private void RibbonWin_Loaded(object sender, RoutedEventArgs e)
        {

            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }

        }

        public event Action AdjointButtonClicked;
        public event Action LoadButtonClicked;

        private void adjointButton_Click(object sender, RoutedEventArgs e)
        {
            AdjointButtonClicked?.Invoke();
            //adjointButton.chec = true;
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            LoadButtonClicked?.Invoke();
        }
        public event Action SelectionButtonClicked;

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            SelectionButtonClicked?.Invoke();

        }
        public event Action FiltAllButtonClicked;

        private void RibbonButton_Click_2(object sender, RoutedEventArgs e)
        {
            FiltAllButtonClicked?.Invoke();
        }
    }
}

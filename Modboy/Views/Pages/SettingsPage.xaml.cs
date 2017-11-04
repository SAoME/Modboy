// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <SettingsPage.xaml.cs>
//  Created By: Alexey Golub
//  Date: 24/02/2016
// ------------------------------------------------------------------ 

using System.Windows;
using System.Windows.Controls;

namespace Modboy.Views.Pages
{
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void AliasDataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid) sender;
            grid.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
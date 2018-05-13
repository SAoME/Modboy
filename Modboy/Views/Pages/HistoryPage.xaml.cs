// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <HistoryPage.xaml.cs>
//  Created By: Alexey Golub
//  Date: 06/03/2016
// ------------------------------------------------------------------ 

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Modboy.Views.Pages
{
    public partial class HistoryPage
    {
        public HistoryPage()
        {
            InitializeComponent();
        }

		private void OnRequestNavigateHyperlink(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			var destination = ((Hyperlink)e.OriginalSource).NavigateUri;

			if (string.IsNullOrWhiteSpace(destination?.AbsoluteUri))
			{
				return;
			}
			try
			{
				Process.Start(destination.AbsoluteUri);
			}
			catch (Exception ex)
			{
				Logger.Record($"Could not open {destination.AbsoluteUri}");
				Logger.Record(ex);
			}
		}
	}
}
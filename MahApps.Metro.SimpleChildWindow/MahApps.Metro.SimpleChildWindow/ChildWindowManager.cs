﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace MahApps.Metro.SimpleChildWindow
{
	public static class ChildWindowManager
	{
		/// <summary>
		/// An enumeration to control the fill behavior of the behavior
		/// </summary>
		public enum OverlayFillBehavior
		{
			/// <summary>
			/// The overlay covers the full window
			/// </summary>
			FullWindow,
			/// <summary>
			/// The overlay covers only then window content, so the window taskbar is useable
			/// </summary>
			WindowContent
		}

		/// <summary>
		/// Shows the given child window asynchronous.
		/// </summary>
		/// <param name="window">The owning window with a container of the child window.</param>
		/// <param name="dialog">A child window instance.</param>
		/// <param name="overlayFillBehavior">The overlay fill behavior.</param>
		/// <returns>
		/// A task representing the operation.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">
		/// The provided child window can not add, the container can not be found.
		/// or
		/// The provided child window is already visible in the specified window.
		/// </exception>
		public static Task ShowChildWindowAsync(this MetroWindow window, ChildWindow dialog, OverlayFillBehavior overlayFillBehavior = OverlayFillBehavior.WindowContent)
		{
			window.Dispatcher.VerifyAccess();
			var metroDialogContainer = window.Template.FindName("PART_MetroDialogContainer", window) as Grid;
			if (metroDialogContainer == null)
			{
				throw new InvalidOperationException("The provided child window can not add, the container can not be found.");
			}
			if (metroDialogContainer.Children.Contains(dialog))
			{
				throw new InvalidOperationException("The provided child window is already visible in the specified window.");
			}

			if (overlayFillBehavior == OverlayFillBehavior.WindowContent)
			{
				metroDialogContainer.SetValue(Grid.RowProperty, 1);
				metroDialogContainer.SetValue(Grid.RowSpanProperty, 1);
			}

			return Task.Factory
					   .StartNew(() => dialog.Dispatcher.Invoke(new Action(() => metroDialogContainer.Children.Add(dialog))))
					   .ContinueWith(_ => dialog.Dispatcher.Invoke(new Func<Task>(() => {
						   var tcs = new TaskCompletionSource<object>();
						   RoutedEventHandler handler = null;
						   handler = (sender, args) => {
							   dialog.ClosingFinished -= handler;
							   metroDialogContainer.Children.Remove(dialog);
							   tcs.TrySetResult(null);
						   };
						   dialog.ClosingFinished += handler;
						   dialog.IsOpen = true;
						   return tcs.Task;
					   })));
		}
	}
}

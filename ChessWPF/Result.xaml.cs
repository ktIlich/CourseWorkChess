using System.Windows;
using System.Windows.Controls;
using ChessLib;

namespace ChessWPF {
  public partial class Result : Window {
    Chess _chess;
    MainWindow _main;

    public Result( Chess chess , MainWindow parent ) {
      InitializeComponent( );
      _chess = chess;
      _main = parent;
    }

    public void Init( bool mate, bool stale ) {
      Label label = new Label( );
      if ( mate ) {
        label.Content = _chess.GetCurrentColor( ) == "Black" ? "Победили черные" : "Победили белые";
        label.FontSize = 25;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        res_grid.Children.Add( label );
        Grid.SetRow( label , 1 );
        Grid.SetRowSpan( label , 2 );
      }
      if ( stale ) {
        label.Content = "Ничья";
        label.FontSize = 25;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        res_grid.Children.Add( label );
        Grid.SetRow( label , 1 );
        Grid.SetRowSpan( label , 2 );
      }
    }

    private void New_game_Click( object sender , RoutedEventArgs e ) {
      _main.ListViewMenu.SelectedIndex = 1;
      this.Hide( );
    }

    private void Exit_Click( object sender , RoutedEventArgs e ) {
      App.Current.Shutdown( 0 );
    }
  }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ChessLib;

namespace ChessWPF {
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    static Chess game;
    static Board board;
    static MoveViewer viewer;
    static ToolBar toolBar;
    static DispatcherTimer dispatcherTimer;
    static Setting setting;
    static NetBoard netBoard;
    static Result result;

    public MainWindow( ) {

      game = new Chess( "rnbq1rk1/ppp2pbp/8/3pp3/8/3P4/PPPBQPPP/RN2KBNR w - - 0 8" );

      board = new Board( );
      board.GSChess = game;

      toolBar = new ToolBar( board );
      board.GSToolBar = toolBar;

      board.SetFigureChess( );

      viewer = new MoveViewer( );
      viewer.Board = board;
      viewer.Chess = game;

      InitializeComponent( );

      dispatcherTimer = new DispatcherTimer( TimeSpan.FromSeconds( 1 ) ,
        DispatcherPriority.Normal , new EventHandler( dispatcherTimer_Tick ) ,
        Dispatcher );
      dispatcherTimer.Start( );

      setting = new Setting( this );
      result = new Result( game , this );
      board.Result = result;

      netBoard = new NetBoard( );
    }

    public void NewGame( int index ) {
      game = new Chess( "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1" );
      board = new Board( );
      board.GSChess = game;
      toolBar = new ToolBar( board );
      board.GSToolBar = toolBar;
      switch ( index ) {
        case 0: {
          board.WithBot = false;
          break;
        }
        case 1: {
          board.WithBot = true;
          board.Diff = 1;
          break;
        }
        case 2: {
          board.WithBot = true;
          board.Diff = 2;
          break;
        }
        case 3: {
          board.WithBot = true;
          board.Diff = 3;
          break;
        }
        case 4: {
          board.WithBot = true;
          board.Diff = 4;
          break;
        }
      }
      board.SetFigureChess( );
      viewer = new MoveViewer( );
      viewer.Board = board;
      viewer.Chess = game;
      viewer.Reset( );
      game.MoveStack.Clear( );
      game.FenStack.Clear( );
      dispatcherTimer = new DispatcherTimer( TimeSpan.FromSeconds( 1 ) ,
      DispatcherPriority.Normal , new EventHandler( dispatcherTimer_Tick ) ,
      Dispatcher );
      dispatcherTimer.Start( );
      setting = new Setting( this );
      result = new Result( game , this );
      board.Result = result;
      ListViewMenu.SelectedIndex = 0;
      netBoard = new NetBoard( );
    }



    public MainWindow GetMain( ) {
      return this;
    }

    private void main_window_loaded( object sender , RoutedEventArgs e ) {
      double pos = ( GridMenu.ActualHeight - ListViewMenu.ActualHeight ) / 2;

      double temp = content.ActualWidth / 3;
      toolBar.Undo.Margin = new Thickness( temp , 0 , 0 , 0 );

      MoveCursorMenu( 0 , pos );

      board.GSMainWindow = this;

      board.GameTimer.PlayerColor = game.GetCurrentColor( );
      board.GameTimer.Enabled = game.IsCheck( ) || game.GetAllMoves( ).Count > 0;
    }

    private void MoveCursorMenu( int index , double pos ) {
      TrainsitionigContentSlide.OnApplyTemplate( );
      GridCursor.Margin = new Thickness( 0 , ( pos + ( index * 60 ) ) , 0 , 0 );
    }

    private void ListViewMenu_SelectionChanged( object sender , SelectionChangedEventArgs e ) {
      int index = ListViewMenu.SelectedIndex;
      double pos = ( GridMenu.ActualHeight - ListViewMenu.ActualHeight ) / 2;

      MoveCursorMenu( index , pos );
      double temp = content.ActualWidth / 3;
      toolBar.Undo.Margin = new Thickness( temp , 0 , 0 , 0 );

      switch ( index ) {
        case 0:
          Grid gr = new Grid( );
          gr.Width = 120;

          viewer.Margin = new Thickness( 5 , 0 , 0 , 0 );
          board.Margin = new Thickness( 5 , 0 , 5 , 0 );
          
          content.Children.Clear( );
          content.Children.Add( toolBar );
          content.Children.Add( viewer );
          content.Children.Add( gr );
          content.Children.Add( board );

          DockPanel.SetDock( toolBar , Dock.Top );
          DockPanel.SetDock( viewer , Dock.Left );
          DockPanel.SetDock( gr , Dock.Right );

          if( dispatcherTimer != null ){
            dispatcherTimer.Start( );
          }

          break;
        case 1:
          content.Children.Clear( );
          content.Children.Add( setting );
          break;
        case 2:
          content.Children.Clear( );
          content.Children.Add( netBoard );
          dispatcherTimer.Stop( );
          break;
        default:
          break;
      }
    }

    private void Window_SizeChanged( object sender , SizeChangedEventArgs e ) {
      int index = ListViewMenu.SelectedIndex;
      double pos = ( GridMenu.ActualHeight - ListViewMenu.ActualHeight ) / 2;

      double temp = content.ActualWidth / 3;
      toolBar.Undo.Margin = new Thickness( temp , 0 , 0 , 0 );

      board.Width = board.ActualHeight;

      MoveCursorMenu( index , pos );
    }

    private void dispatcherTimer_Tick( object sender , EventArgs e ) {
      if ( game.GetAllMoves( ).Count > 0 ) {
        GameTimer gameTimer;
        gameTimer = board.GameTimer;
        toolBar.labelWhitePlayTime.Content = GameTimer.GetHumanElapse( gameTimer.WhitePlayTime );
        toolBar.labelBlackPlayTime.Content = GameTimer.GetHumanElapse( gameTimer.BlackPlayTime );

        if ( board.WithBot ) {
          board.MoveBot( );
        }


      }
    }

    private void Window_Closing( object sender , System.ComponentModel.CancelEventArgs e ) {
      dispatcherTimer.Stop( );
      App.Current.Shutdown( );
    }
  }
}

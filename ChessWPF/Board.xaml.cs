using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ChessLib;

namespace ChessWPF {

  public class ChessCommands {
    static ChessCommands( ) {
      UndoCommand = new RoutedUICommand( "_Undo" , "Undo" , typeof( ToolBar ) );
      RedoCommand = new RoutedUICommand( "_Redo" , "Redo" , typeof( ToolBar ) );
    }
    public static RoutedCommand UndoCommand { get; set; }
    public static RoutedCommand RedoCommand { get; set; }
  }

  public class NewMoveEventArgs : EventArgs {
    public string Move { get; private set; }

    public NewMoveEventArgs( string Move ) {
      this.Move = Move;
    }
  }

  public partial class Board : UserControl {

    public event EventHandler<EventArgs> BoardReset;
    public event EventHandler<NewMoveEventArgs> NewMove;
    public event EventHandler RedoPosChanged;
    public event EventHandler<NewMoveEventArgs> NewMoveNet;

    public RoutedUICommand UndoCommand;
    public RoutedUICommand RedoCommand;

    private Chess _chess;
    private Color LightColor;
    private Color DarkColor;
    private Border[ , ] board_map;
    private GameTimer game_timer;
    private ToolBar toolBar;
    private MainWindow main;
    Bot bot = new Bot( );


    private bool wait { get; set; } = true;
    public bool WithBot { get; set; } = false;
    public int Diff { get; set; }
    public bool WithOnline { get; set; }
    public string strResult { get; set; }

    int xFrom, yFrom;


    public Chess GSChess {
      get => _chess;
      set { _chess = value != null ? value : new Chess( ); }
    }

    public ToolBar GSToolBar {
      get => toolBar;
      set { toolBar = value != null ? value : new ToolBar( );  }
    }

    public MainWindow GSMainWindow {
      get => main;
      set { main = value; }
    }

    public Result Result { get; set; }

    public GameTimer GameTimer {
      get => game_timer;
    }



    public Board( ) {
      InitializeComponent( );

      LightColor = ( Color )ColorConverter.ConvertFromString( "#eeeed2" );
      DarkColor = ( Color )ColorConverter.ConvertFromString( "#769656" );

      board_map = new Border[ 8 , 8 ];

      CreateChessBoard( );

      WithOnline = false;

      game_timer = new GameTimer( );
      game_timer.Enabled = false;

      UndoCommand = new RoutedUICommand( "_Undo" , "Undo" , typeof( MainWindow ) );
      RedoCommand = new RoutedUICommand( "_Redo" , "Redo" , typeof( MainWindow ) );
    }

    public Board( Chess chess ) {
      InitializeComponent( );
      _chess = chess;
      LightColor = ( Color )ColorConverter.ConvertFromString( "#eeeed2" );
      DarkColor = ( Color )ColorConverter.ConvertFromString( "#769656" );
      board_map = new Border[ 8 , 8 ];
      CreateChessBoard( );
      WithOnline = false;
      game_timer = new GameTimer( );
      game_timer.Enabled = false;
      game_timer.Reset( _chess.GetCurrentColor( ) );
      UndoCommand = new RoutedUICommand( "_Undo" , "Undo" , typeof( MainWindow ) );
      RedoCommand = new RoutedUICommand( "_Redo" , "Redo" , typeof( MainWindow ) );
    }



    private void CreateChessBoard( ) {
      Border cell;
      Brush brushDark = new SolidColorBrush( DarkColor );
      Brush brushLight = new SolidColorBrush( LightColor );

      for ( int y = 7; y >= 0; y-- ) {
        for ( int x = 0; x < 8; x++ ) {
          cell = new Border( );
          cell.Name = "C" + x + y;
          cell.BorderThickness = new Thickness( 0 );
          cell.Background = ( ( ( x + y ) % 2 ) == 0 ) ? brushLight : brushDark;
          cell.BorderBrush = cell.Background;
          cell.MouseDown += Cell_MouseDown;
          board_map[ x , y ] = cell;
          ChessBoard.Children.Add( cell );
        }
      }
    }

    public void SetFigureChess( ) {
      for ( int y = 7; y >= 0; y-- ) {
        for ( int x = 0; x < 8; x++ ) {
          SetFigure( x , y , _chess.GetFigureAt( x , y ) );
        }
      }
      MarkCell( );
      if ( !WithOnline ) {
        if ( _chess.IsCheckmate( ) || _chess.IsStalemate( ) ) {
          Result.Init( _chess.IsCheckmate( ) , _chess.IsStalemate( ) );
          Result.ShowDialog( );
          this.IsEnabled = false;
        }
        if ( _chess.GetAllMoves( ).Count > 0 && this.IsEnabled == false ) {
          this.IsEnabled = true;
        }
      }
    }

    public void UndoMove( ) {
      if ( !this.WithBot ) {
        _chess = _chess.UndoMove( );
        SetFigureChess( );
      }
      else {
        _chess = _chess.UndoMove( );
        SetFigureChess( );
        _chess = _chess.UndoMove( );
        SetFigureChess( );
      }
      game_timer.PlayerColor = _chess.GetCurrentColor( );
      game_timer.Enabled = true;
    }

    public void RedoMove( ) {
      if ( !this.WithBot ) {
        _chess = _chess.RedoMove( );
        SetFigureChess( );
      }
      else {
        _chess = _chess.RedoMove( );
        SetFigureChess( );
        _chess = _chess.RedoMove( );
        SetFigureChess( );
      }
      game_timer.PlayerColor = _chess.GetCurrentColor( );
      game_timer.Enabled = _chess.IsCheck( ) || _chess.GetAllMoves( ).Count > 0;
    }

    private void SetFigure( int x , int y , char figure ) {
      Border cell;
      UserControl cell_figure;
      cell = board_map[ x , y ];
      cell_figure = new UserControl( );

      if ( cell_figure != null && figure != '.' ) {
        cell_figure.Content = GetFigure( figure );

        cell_figure.Margin = ( cell.BorderThickness.Top == 0 ) ? new Thickness( 1 ) : new Thickness( 3 );
      }
      cell.Child = cell_figure;
    }

    private Image GetFigure( Char figure ) {
      switch ( figure ) {
        case 'P': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/WhitePawn.png" , UriKind.Relative ) ); return img; }
        case 'p': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/BlackPawn.png" , UriKind.Relative ) ); return img; }
        case 'N': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/WhiteKnight.png" , UriKind.Relative ) ); return img; }
        case 'n': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/BlackKnight.png" , UriKind.Relative ) ); return img; }
        case 'B': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/WhiteBishop.png" , UriKind.Relative ) ); return img; }
        case 'b': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/BlackBishop.png" , UriKind.Relative ) ); return img; }
        case 'R': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/WhiteRook.png" , UriKind.Relative ) ); return img; }
        case 'r': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/BlackRook.png" , UriKind.Relative ) ); return img; }
        case 'Q': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/WhiteQueen.png" , UriKind.Relative ) ); return img; }
        case 'q': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/BlackQueen.png" , UriKind.Relative ) ); return img; }
        case 'K': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/WhiteKing.png" , UriKind.Relative ) ); return img; }
        case 'k': { Image img = new Image( ); img.Source = new BitmapImage( new Uri( $"/Resources/BlackKing.png" , UriKind.Relative ) ); return img; }
        default: return null;
      }
    }

    public void DoMove( string move ) {
      if ( _chess.GetAllMoves( ).Count > 0 ) {
        _chess = _chess.Move( move );
        OnNewMove( new NewMoveEventArgs( move ) );
        if ( WithOnline ) {
          OnNewMoveNet( new NewMoveEventArgs( move ) );
        }
      }
    }

    public void DoMoveNet( string move ) {
      if ( _chess.GetAllMoves( ).Count > 0 ) {
        _chess = _chess.Move( move );
        OnNewMove( new NewMoveEventArgs( move ) );
      }
    }

    public void MoveBot( ) {
      if ( _chess.GetCurrentColor( ) == "Black" && _chess.GetAllMoves( ).Count > 0 ) {
        string move = GetMoveBot( );
        string xy = move.Substring( 3 , 2 );
        int x = xy[ 0 ] - 'a';
        int y = xy[ 1 ] - '1';

        DoMove( move );

        UserControl ctrl = ( UserControl )board_map[ x , y ].Child;
        ctrl.Margin = new Thickness( 2 );
        ctrl.BorderBrush = ( ( ( x + y ) % 2 ) == 0 ) ? Brushes.DarkGreen : Brushes.Beige;
        ctrl.BorderThickness = new Thickness( 2 );

        SetFigureChess( );
        game_timer.PlayerColor = _chess.GetCurrentColor( );
        game_timer.Enabled = _chess.IsCheck( ) || _chess.GetAllMoves( ).Count > 0;
      }
    }

    private string GetMoveBot( ) {
      return bot.GetBestMove( _chess , Diff );
    }

    private void MarkCellFrom( ) { // Pe2e4 xy = e2
      foreach ( string move in _chess.GetAllMoves( ) ) {
        string xy = move.Substring( 1 , 2 );
        int x = xy[ 0 ] - 'a';
        int y = xy[ 1 ] - '1';

        UserControl ctrl = ( UserControl )board_map[ x , y ].Child;

        ctrl.Margin = new Thickness( 2 );
        ctrl.BorderBrush = Brushes.Black;
        ctrl.BorderThickness = new Thickness( 2 );
      }
    }

    private void RefreshBoard( ) {
      for ( int y = 7; y >= 0; y-- ) {
        for ( int x = 0; x < 8; x++ ) {
          UserControl control = ( UserControl )board_map[ x , y ].Child;
          control.Margin = new Thickness( 0 );
          control.BorderThickness = new Thickness( 0 );
        }
      }
    }

    public string ToCoord( int x , int y ) {
      return ( ( char )( 'a' + x ) ).ToString( ) + ( ( char )( '1' + y ) ).ToString( );
    }

    private void MarkCell( ) {
      RefreshBoard( );
      if ( wait ) {
        MarkCellFrom( );
      }
      else {
        MarkCellTo( );
      }
    }

    private void MarkCellTo( ) { // Pe2e4 xy = e4
      string suffix = _chess.GetFigureAt( xFrom , yFrom ) + ToCoord( xFrom , yFrom );
      foreach ( string move in _chess.GetAllMoves( ) ) {
        if ( move.StartsWith( suffix ) ) {
          string xy = move.Substring( 3 , 2 );
          int x = xy[ 0 ] - 'a';
          int y = xy[ 1 ] - '1';

          UserControl ctrl = ( UserControl )board_map[ x , y ].Child;
          ctrl.Margin = new Thickness( 2 );
          ctrl.BorderBrush = ( ( ( x + y ) % 2 ) == 0 ) ? Brushes.DarkGreen : Brushes.Beige;
          ctrl.BorderThickness = new Thickness( 2 );
        }
      }
    }





    public void Cell_MouseDown( object sender , RoutedEventArgs e ) {
      Border cell_select = ( Border )sender;
      UserControl cell_figure = ( UserControl )cell_select.Child;
      string xy = cell_select.Name.Substring( 1 );
      int x = int.Parse( xy[ 0 ].ToString( ) );
      int y = int.Parse( xy[ 1 ].ToString( ) );
      if ( wait ) {
        wait = false;
        xFrom = x;
        yFrom = y;
      }
      else {
        wait = true;

        if ( _chess.GetCurrentMoveNumber( ) > 0 ) {
          string figure = _chess.GetFigureAt( xFrom , yFrom ).ToString( );
          string move = figure + ToCoord( xFrom , yFrom ) + ToCoord( x , y );

          if ( _chess.GetCurrentColor( ) == "White" ) {
            if ( y == 7 && figure == "P" ) {
              Promotion pr = new Promotion( "White" );
              pr.ShowDialog( );
              move += pr.FigurePromotion;
            }
          }
          else if ( _chess.GetCurrentColor( ) == "Black" ) {
            if ( y == 0 && figure == "p" ) {
              Promotion pr = new Promotion( "Black" );
              pr.ShowDialog( );
              move += pr.FigurePromotion;
            }
          }
          DoMove( move );
        }
      }
      SetFigureChess( );
      game_timer.PlayerColor = _chess.GetCurrentColor( );
      game_timer.Enabled = _chess.IsCheck( ) || _chess.GetAllMoves( ).Count > 0;
    }




    protected void OnRedooPosChanged( EventArgs e ) {
      if ( RedoPosChanged != null ) {
        RedoPosChanged( this , e );
      }
    }

    protected void OnNewMove( NewMoveEventArgs e ) {
      if ( NewMove != null ) {
        NewMove( this , e );
      }
    }

    protected void OnNewMoveNet( NewMoveEventArgs e ) {
      if ( NewMoveNet != null ) {
        NewMoveNet( this , e );
      }
    }

    protected void OnBoardReset( EventArgs e ) {
      if ( BoardReset != null ) {
        BoardReset( this , e );
      }
    }
  }
}

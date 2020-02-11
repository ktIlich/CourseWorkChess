using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChessWPF {

  public partial class Promotion : Window {

    char[ ] arr_fig_symb = { 'P', 'R', 'N', 'B', 'Q',
                            'p', 'r', 'n', 'b', 'q' };

    public char FigurePromotion { get; private set; }

    public Promotion( string color ) {
      InitializeComponent( );
      CreateWin( color );
    }

    private void CreateWin( string Color ) {
      if ( Color == "White" ) {
        for ( int i = 0; i < 5; i++ ) {
          StackPanel panel = new StackPanel( );
          panel.Name = arr_fig_symb[ i ] + "_panel";
          Image img = GetFigure( arr_fig_symb[ i ] );
          Label caption = new Label( );
          caption.Content = GetFigureName( arr_fig_symb[ i ] );
          caption.HorizontalAlignment = HorizontalAlignment.Center;
          panel.VerticalAlignment = VerticalAlignment.Center;
          panel.Children.Add( img );
          panel.Children.Add( caption );
          panel.Margin = new Thickness( 10 , 5 , 10 , 5 );
          panel.MouseEnter += Panel_MouseEnter;
          panel.MouseLeave += Panel_MouseLeave;
          panel.MouseDown += Panel_MouseDown;
          FigureStack.Children.Add( panel );
        }
      }
      else if ( Color == "Black" ) {
        for ( int i = 5; i < 10; i++ ) {
          StackPanel panel = new StackPanel( );
          panel.Name = arr_fig_symb[ i ] + "_panel";
          Image img = GetFigure( arr_fig_symb[ i ] );
          Label caption = new Label( );
          caption.Content = GetFigureName( arr_fig_symb[ i ] );
          caption.HorizontalAlignment = HorizontalAlignment.Center;
          panel.VerticalAlignment = VerticalAlignment.Center;
          panel.Children.Add( img );
          panel.Children.Add( caption );
          panel.Margin = new Thickness( 10 , 5 , 10 , 5 );
          panel.MouseEnter += Panel_MouseEnter;
          panel.MouseLeave += Panel_MouseLeave;
          panel.MouseDown += Panel_MouseDown;
          FigureStack.Children.Add( panel );
        }
      }
    }

    private void Panel_MouseLeave( object sender , MouseEventArgs e ) {
      ( ( Panel )sender ).Background = new SolidColorBrush( Color.FromRgb( 255 , 255 , 255 ) );
      Cursor = Cursors.Arrow;
    }

    private void Panel_MouseEnter( object sender , MouseEventArgs e ) {
      Panel panel = ( ( Panel )sender );
      panel.Background = new SolidColorBrush( Color.FromArgb( 50 , 68 , 68 , 68 ) );
      Cursor = Cursors.Hand;
    }

    private void Panel_MouseDown( object sender , MouseButtonEventArgs e ) {
      FigurePromotion = ( ( Panel )sender ).Name.Substring( 0 , 1 ).ToCharArray( )[ 0 ];
      DialogResult = true;
      Close( );
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

    private string GetFigureName( Char figure ) {
      switch ( figure ) {
        case 'P': return "Pawn";
        case 'p': return "Pawn";
        case 'N': return "Knight";
        case 'n': return "Knight";
        case 'B': return "Bishop";
        case 'b': return "Bishop";
        case 'R': return "Rook";
        case 'r': return "Rook";
        case 'Q': return "Queen";
        case 'q': return "Queen";
        case 'K': return "King";
        case 'k': return "King";
        default: return null;
      }
    }

    private void Button_Click( object sender , RoutedEventArgs e ) {
      DialogResult = true;
      Close( );
    }
  }
}

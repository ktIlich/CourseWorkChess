using System.Windows.Controls;

namespace ChessWPF {
  /// <summary>
  /// Логика взаимодействия для ToolBar.xaml
  /// </summary>
  public partial class ToolBar : UserControl {

    Board Board;
    public Button UndoBtn { get; set; }
    public Button RedoBtn { get; set; }
    public ToolBar( ) {
      InitializeComponent( );
    }

    public ToolBar( Board board ) {
      InitializeComponent( );
      Board = board;
      UndoBtn = Undo;
      RedoBtn = Redo;
    }



    private void Undo_Click( object sender , System.Windows.RoutedEventArgs e ) {
      Board.UndoMove( );
    }

    private void Redo_Click( object sender , System.Windows.RoutedEventArgs e ) {
      Board.RedoMove( );
    }
  }
}

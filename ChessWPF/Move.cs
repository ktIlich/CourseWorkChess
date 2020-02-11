namespace ChessWPF {
  public class Move {
    public int MOVE_ID { get; set; }

    public int MOVE_NUMBER { get; set; }

    public string PLAYER_COLOR { get; set; }

    public string MOVE1 { get; set; }

    public string FEN { get; set; }

    public Move( int number , string color , string move , string fen ) {
      MOVE_ID = number;
      MOVE_NUMBER = number;
      PLAYER_COLOR = color;
      MOVE1 = move;
      FEN = fen;
    }
  }
}

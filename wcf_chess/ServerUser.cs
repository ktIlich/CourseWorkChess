using System;
using System.ServiceModel;
using ChessLib;

namespace wcf_chess {

  enum StatusGame {
    wait, play, done
  }

  enum ResultGame {
    white_win, black_win,
    white_exit, black_exit,
    draw, none
  }

  class ServerUser {
    public int ID { get; set; }
    public string Name { get; set; }
    public DateTime ConnectTime { get; set; }
    public bool IsAdmin { get; set; } // 1 - admin 0 - player
    public OperationContext operationContext { get; set; }
  }


  class ServerSession {
    public int ID_G { get; set; }
    public int Admin_ID { get; set; }
    public int White_P { get; set; }
    public int Black_P { get; set; }
    public StatusGame Status { get; set; }
    public Chess Game { get; set; }
    public string FEN { get; set; }
    public string LastMove { get; set; }
    public ResultGame Result { get; set; }
    //public OperationContext operationContext { get; set; }
  }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ChessLib;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity.Validation;
using System.Data.SqlClient;

namespace wcf_chess {

  [ServiceBehavior( InstanceContextMode = InstanceContextMode.Single , IncludeExceptionDetailInFaults = true )]
  public class ServiceChess : IServiceChess {

    static ChessDB db = new ChessDB( );

    List<ServerUser> users = new List<ServerUser>( );
    LinkedList<ServerUser> queue_users = new LinkedList<ServerUser>( );
    List<ServerSession> games = new List<ServerSession>( );

    static int nextId = db.Players.Count( ) + 1;
    static int GameID = db.Games.Count( ) + 1;
    static int moveID = 0;


    public int Connect( string name ) {
      ServerUser user = new ServerUser( ) {
        ID = nextId ,
        Name = name ,
        ConnectTime = DateTime.Now ,
        operationContext = OperationContext.Current
      };
      nextId++;
      users.Add( user );

      Player player = new Player( );
      player.Player_ID = user.ID;
      player.Player_Name = user.Name;
      db.Players.Add( player );

      db.SaveChanges( );

      return user.ID;
    }

    public int CreateSession( int id ) {
      ServerSession session = new ServerSession( ) {
        ID_G = GameID ,
        Admin_ID = id ,
        Status = StatusGame.wait ,
      };
      var admin = users.FirstOrDefault( p => p.ID == id );
      if ( admin != null ) {
        admin.IsAdmin = true;
      }
      GameID++;
      games.Add( session );
      queue_users.AddLast( admin );
      return session.ID_G;
    }

    public void ConnectSession( int id ) {
      ServerUser wait_user = users.FirstOrDefault( p => p.ID == id );
      if ( wait_user != null ) {
        wait_user.IsAdmin = false;
        queue_users.AddLast( wait_user );
      }
    }

    public void SetColor( int g_id , string color ) {
      var session = games.FirstOrDefault( p => p.ID_G == g_id );
      if ( session != null ) {
        switch ( color ) {
          case "White": session.White_P = session.Admin_ID; session.Black_P = 0; break;
          case "Black": session.Black_P = session.Admin_ID; session.White_P = 0; break;
          default: session.White_P = session.Admin_ID; session.Black_P = 0; break;
        }
      }
    }

    public string GetColor( int id , int g_id ) {
      var session = games.FirstOrDefault( p => p.ID_G == g_id );
      string color = session.White_P == id ? "White" : "Black";
      return color;
    }

    public int[ ] GetOnlineCount( ) {
      int player_count_queue = queue_users.Count( );
      int wait_games = games.Count( );
      int wait_players = queue_users.Where( p => p.IsAdmin == false ).Count( );
      return new int[ ] { player_count_queue , wait_games , wait_players };
    }

    public int StartGame( int id ) {
      var user_check_1 = users.FirstOrDefault( p => p.ID == id );
      if ( user_check_1 != null ) {
        var user_check_2 = queue_users.FirstOrDefault( p => p.ID == id );
        if ( user_check_2 != null ) {
          if ( user_check_2.IsAdmin ) {
            var player = queue_users.FirstOrDefault( p => p.IsAdmin == false );
            if ( player != null ) {
              var game = games.FirstOrDefault( p => p.Admin_ID == id && p.Status == StatusGame.wait );
              if ( game != null ) {
                game.Game = new Chess( );
                if ( game.Black_P == 0 ) {
                  game.Black_P = player.ID;
                }
                else if ( game.White_P == 0 ) {
                  game.White_P = player.ID;
                }
                game.FEN = game.Game.fen;
                game.Status = StatusGame.play;
                game.Result = ResultGame.none;

              

                return game.ID_G;
              }
            }
          }
          else {
            var game = games.FirstOrDefault( p => p.Status == StatusGame.wait );
            if ( game != null ) {
              return game.ID_G;
            }
            var game_2 = games.FirstOrDefault( p => p.Black_P == id || p.White_P == id );
            if ( game_2 != null ) {
              return game_2.ID_G;
            }
          }
        }
      }
      return -1;
    }


    public string GetGameInfo( int g_id ) {
      var game = games.FirstOrDefault( p => p.ID_G == g_id );
      if ( game != null ) {
        var white_p = users.FirstOrDefault( p => p.ID == game.White_P );
        var black_p = users.FirstOrDefault( p => p.ID == game.Black_P );
        if ( white_p != null && black_p != null ) {
          string info = $" ID игры : { game.ID_G }\n Белый игрок : { white_p.Name }\n Черный игрок : { black_p.Name }";
          return info;
        }
      }
      return "";
    }

    public string EndGame( int g_id ) {
      ServerSession game = games.FirstOrDefault( p => p.ID_G == g_id );
      if ( game != null ) {
        if ( game.Game.IsCheckmate( ) ) {
          string win_color = game.Game.GetCurrentColor( ) == "Black" ? "Победили черные" : "Победили белые";
          game.Result = game.Game.GetCurrentColor( ) == "Black" ? ResultGame.black_win : ResultGame.white_win;
          game.Status = StatusGame.done;

          //Game game_ = db.Games.FirstOrDefault( p => p.Game_ID == g_id );
          //game_.Game_Result = game.Game.GetCurrentColor( ) == "Black" ? ( int )ResultGame.black_win : ( int )ResultGame.white_win;
          //game_.Game_Status = ( int )game.Status;
          //db.SaveChanges( );
          return win_color;
        }
        if ( game.Game.IsStalemate( ) ) {
          string draw = "Ничья";
          game.Result = ResultGame.draw;
          game.Status = StatusGame.done;
          //db.Games.FirstOrDefault( p => p.Game_ID == g_id ).Game_Result = ( int )ResultGame.draw;
          //db.Games.FirstOrDefault( p => p.Game_ID == g_id ).Game_Status = ( int )StatusGame.done;
          //db.SaveChanges( );
          return draw;
        }
      }
      return "";
    }

    public void LeaveSession( int id , int g_id ) {
      var game = games.FirstOrDefault( p => p.ID_G == g_id );
      var user = users.FirstOrDefault( p => p.ID == id );
      if ( game != null ) {
        if ( game.Admin_ID == id ) {
          if ( game.Status == StatusGame.wait ) {
            games.Remove( game );
          }
        }
        if ( game.Status == StatusGame.play ) {
          SendMesg( "Ваш оппонент покинул игру :/" , id , g_id );
          game.Status = StatusGame.done;
          game.Result = ResultGame.black_exit;
          //db.Games.FirstOrDefault( p => p.Game_ID == g_id ).Game_Result = ( int )ResultGame.black_exit;
          //db.Games.FirstOrDefault( p => p.Game_ID == g_id ).Game_Status = ( int )StatusGame.done;
          //db.SaveChanges( );
        }
      }
    }


    public void Disconnect( int id ) {

      var user = users.FirstOrDefault( i => i.ID == id );
      if ( user != null ) {

        var game_with_user_wait = games.FirstOrDefault( p => ( p.Black_P == id || p.White_P == id ) && p.Status == StatusGame.wait );
        if ( game_with_user_wait != null ) {
          games.Remove( game_with_user_wait );
        }
        var game_with_user_play = games.FirstOrDefault( p => ( p.Black_P == id || p.White_P == id ) && p.Status == StatusGame.play );
        if ( game_with_user_play != null ) {
          game_with_user_play.Status = StatusGame.done;
          game_with_user_play.Result = ResultGame.white_exit;
          //db.Games.FirstOrDefault( p => p.Game_ID == game_with_user_play.ID_G ).Game_Result = ( int )ResultGame.white_exit;
          //db.Games.FirstOrDefault( p => p.Game_ID == game_with_user_play.ID_G ).Game_Status = ( int )StatusGame.done;
          //db.SaveChanges( );
        }
        var queue_user = queue_users.FirstOrDefault( p => p.ID == id );
        if ( queue_user != null ) {
          queue_users.Remove( queue_user );
        }
        users.Remove( user );
      }
    }

    public void SendMesg( string msg , int id , int g_id ) {
      if ( g_id == 0 ) {
        foreach ( var user in users ) {
          user.operationContext.GetCallbackChannel<IServerChessCallback>( ).MsgCallback( msg );
        }
      }
      else {
        var game = games.FirstOrDefault( p => p.ID_G == g_id );
        if ( game != null ) {
          int p_1 = game.White_P;
          int p_2 = game.Black_P;
          var user_1 = users.FirstOrDefault( p => p.ID == p_1 );
          var user_2 = users.FirstOrDefault( p => p.ID == p_2 );

          List<ServerUser> list = new List<ServerUser>( );
          list.Add( user_1 );
          list.Add( user_2 );

          foreach ( var i in list ) {
            string answer = DateTime.Now.ToShortTimeString( );
            var user = users.FirstOrDefault( p => p.ID == id );
            if ( user != null ) {
              answer += ": " + user.Name + " ";
            }
            answer += msg;
            i.operationContext.GetCallbackChannel<IServerChessCallback>( ).MsgCallback( answer );
          }
        }
      }
    }

    public void SendMove( string move , int id , int g_id ) {
      var game = games.FirstOrDefault( p => p.ID_G == g_id );
      if ( game != null ) {
        int player = game.White_P == id ? game.Black_P : game.White_P;
        if ( string.IsNullOrWhiteSpace( move ) ) {
          move = game.Game.MoveStack.CurrentMove;
        }
        else {
          game.Game = game.Game.Move( move );
        }
        game.FEN = game.Game.fen;
        game.LastMove = move;

        db.Games.FirstOrDefault( p => p.Game_ID == g_id ).CURRENT_FEN = game.FEN;

        //Move _move = new Move( );
        //_move.G_ID = g_id;
        //_move.P_ID = id;
        //_move.Move_ID = moveID++;
        //_move.FEN = game.FEN;
        //_move.PLY = moveID;
        //_move.Move_STR = move;
        //db.Moves.Add( _move );

        //db.SaveChanges( );

        var user = users.FirstOrDefault( p => p.ID == player );
        if ( user != null ) {
          user.operationContext.GetCallbackChannel<IServerChessCallback>( ).MoveCallBack( move );
        }
      }
    }


    public string GetFen( int g_id ) {
      string fen = games.FirstOrDefault( p => p.ID_G == g_id ).FEN;
      return fen;
    }

    public void DeleteFromQueue( int id ) {
      var user = queue_users.FirstOrDefault( p => p.ID == id );
      if ( user != null ) {
        queue_users.Remove( user );
      }
    }

    public int CheckState( int g_id ) {
      var game = games.FirstOrDefault( p => p.ID_G == g_id );
      if ( game != null ) {
        int w_p = game.White_P;
        int b_p = game.Black_P;

        var check_w_p = users.FirstOrDefault( p => p.ID == w_p );
        var check_b_p = users.FirstOrDefault( p => p.ID == b_p );

        if ( check_w_p == null ) {
          return 1;
        }
        if ( check_b_p == null ) {
          return -1;
        }
        return 0;
      }
      return 10;
    }


  }
}
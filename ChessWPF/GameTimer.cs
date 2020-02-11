using System;

namespace ChessWPF {
  public class GameTimer {

    private bool _enabled;
    private DateTime timerStart;
    private TimeSpan timeSpanCommitedWhite;
    private TimeSpan timeSpanCommitedBlack;
    private int iMoveIncInSec;
    private string playerColor;

    public GameTimer( ) {
      _enabled = false;
      Reset( "White" );
    }

    private void Commit( ) {
      DateTime now;
      TimeSpan span;
      if ( _enabled ) {
        now = DateTime.Now;
        span = now - timerStart;
        timerStart = now;
        if ( PlayerColor == "White" ) {
          timeSpanCommitedWhite += span;
        }
        else {
          timeSpanCommitedBlack += span;
        }
      }
    }

    public bool Enabled {
      get {
        return ( _enabled );
      }
      set {
        if ( value != _enabled ) {
          if ( value ) {
            timerStart = DateTime.Now;
          }
          else {
            Commit( );
          }
          _enabled = value;
        }
      }
    }

    public void ResetTo( string PlayerColor , long lWhiteTicks , long lBlackTicks ) {
      playerColor = PlayerColor;
      timeSpanCommitedWhite = new TimeSpan( lWhiteTicks );
      timeSpanCommitedBlack = new TimeSpan( lBlackTicks );
      timerStart = DateTime.Now;
    }

    public void Reset( string PlayerColor ) {
      ResetTo( PlayerColor , 0 , 0 );
    }

    public string PlayerColor {
      get {
        return ( playerColor );
      }
      set {
        playerColor = value;
      }
    }

    public TimeSpan WhitePlayTime {
      get {
        Commit( );
        return ( timeSpanCommitedWhite );
      }
    }

    public TimeSpan BlackPlayTime {
      get {
        Commit( );
        return ( timeSpanCommitedBlack );
      }
    }

    public int MoveIncInSec {
      get {
        return ( iMoveIncInSec );
      }
      set {
        iMoveIncInSec = value;
      }
    }

    public static string GetHumanElapse( TimeSpan timeSpan ) {
      string strRetVal;
      int iIndex;

      strRetVal = timeSpan.ToString( );
      iIndex = strRetVal.IndexOf( ':' );
      if ( iIndex != -1 ) {
        iIndex = strRetVal.IndexOf( '.' , iIndex );
        if ( iIndex != -1 ) {
          strRetVal = strRetVal.Substring( 0 , iIndex );
        }
      }
      return ( strRetVal );
    }

  }
}

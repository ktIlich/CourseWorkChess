using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace wcf_chess {

  [ServiceContract( CallbackContract = typeof( IServerChessCallback ) )]
  public interface IServiceChess {
    [OperationContract]
    int Connect( string name );

    [OperationContract]
    void Disconnect( int id );

    [OperationContract]
    int CreateSession( int id );

    [OperationContract]
    void ConnectSession( int id );

    [OperationContract]
    void SetColor( int g_id , string color );

    [OperationContract]
    string GetColor( int id , int g_id );

    [OperationContract]
    int[ ] GetOnlineCount( );

    [OperationContract]
    int StartGame( int id );

    [OperationContract]
    string EndGame( int g_id );

    [OperationContract]
    void LeaveSession( int id , int g_id );

    [OperationContract( IsOneWay = true )]
    void SendMesg( string msg , int id , int g_id );

    [OperationContract( IsOneWay = true )]
    void SendMove( string move , int id , int g_id );

    [OperationContract]
    string GetGameInfo( int g_id );

    [OperationContract]
    string GetFen( int g_id );

    [OperationContract]
    void DeleteFromQueue( int id );

    [OperationContract]
    int CheckState( int g_id );
  }

  public interface IServerChessCallback {
    [OperationContract( IsOneWay = true )]
    void MsgCallback( string msg );

    [OperationContract( IsOneWay = true )]
    void MoveCallBack( string msg );
  }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace ChessHost {
  class Program {
    static void Main( string[ ] args ) {
      using ( var host = new ServiceHost( typeof( wcf_chess.ServiceChess ) ) ) {
        host.Open( );
        Console.WriteLine( "Хост стартовал!" );
        Console.ReadLine( );
      }
    }
  } 
}

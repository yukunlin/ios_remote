using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IverRemoteServer
{
    public interface IverComm
    {
        void UpdateFrontseatData();
        void ClosePort();
        void SendBackseatCommands(int topFin, int bottomFin, int portFin, int starboardFin, int motorSpeed, int timeOut);
        double Heading { get; }
        double[] LatLong { get; }
        double Pitch { get; }
        double Row { get; }
        double Speed { get; }
    }
}

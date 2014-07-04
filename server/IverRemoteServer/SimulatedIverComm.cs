using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IverRemoteServer
{
    class SimulatedIverComm : IverComm
    {

        private double _speed = 0;
        private double _heading = 0;
        private double _pitch = 0;
        private double _row = 0;

        public SimulatedIverComm(string port)
        {
            // Does nothing
        }

        public void ClosePort()
        {
            // Does Nothing 
        }

        public void UpdateFrontseatData()
        {
            // Does nothing
        }

        public void SendBackseatCommands(int topFin, int bottomFin, int portFin, int starboardFin, int motorSpeed, int timeOut)
        {
            _speed = (motorSpeed - 128) / 40.0;
            _pitch =  (motorSpeed - 128) / 25.0;
            _row = _speed * ((128 - topFin) / 20.0);
            _heading = ( _heading - _speed * ((128 - topFin) / 20.0) );

            if (_heading < 0)
            {
                _heading = 360 + _heading;
            }

            _heading = _heading % 360;
        }

        public double Heading
        {
            get { return _heading; }
        }

        public double[] LatLong
        {
            get { return null; }
        }

        public double Pitch
        {
            get { return _pitch; }
        }

        public double Row
        {
            get { return _row; }
        }

        public double Speed
        {
            get { return _speed; }
        }
    }
}

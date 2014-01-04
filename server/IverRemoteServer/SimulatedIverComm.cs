using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IverRemoteServer
{
    class SimulatedIverComm : IverComm
    {

        private double speed_ = 0;
        private double heading_ = 0;
        private double pitch_ = 0;
        private double row_ = 0;

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
            speed_ = (motorSpeed - 128) / 40.0;
            pitch_ =  (motorSpeed - 128) / 25.0;
            row_ = speed_ * ((128 - topFin) / 20.0);
            heading_ = ( heading_ - speed_ * ((128 - topFin) / 20.0) );

            if (heading_ < 0)
            {
                heading_ = 360 + heading_;
            }

            heading_ = heading_ % 360;
        }

        public double Heading
        {
            get { return heading_; }
        }

        public double[] LatLong
        {
            get { return null; }
        }

        public double Pitch
        {
            get { return pitch_; }
        }

        public double Row
        {
            get { return row_; }
        }

        public double Speed
        {
            get { return speed_; }
        }
    }
}

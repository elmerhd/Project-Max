using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Max
{
    public abstract class MaxService
    {
        public void OnStart()
        {
            Type type = this.GetType().UnderlyingSystemType;
            String className = type.Name;
            MaxUtils.PlayWaitingSound();
        }

        public void OnFinished()
        {
            Type type = this.GetType().UnderlyingSystemType;
            String className = type.Name;
            MaxUtils.StopWaitingSound();
        }
        public abstract void StartService();
    }
}

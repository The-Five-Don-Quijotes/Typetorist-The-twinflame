using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Interface
{
    public interface ITyper
    {
        void ResetLine();
        void CheckInput();
        void SetCurrentWord();
        void SetCurrentLine();
    }
}

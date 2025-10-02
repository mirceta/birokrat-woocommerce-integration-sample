using si.birokrat.rtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace si.birokrat.next.common_dll {
    public class BirokratDllInterface : IDisposable {

        SimpleBiroDLLToRTCAdapter _adapter;

        #region [constructor]
        public BirokratDllInterface(string guid) {
            _adapter = new SimpleBiroDLLToRTCAdapter(guid);
        }

        public void Dispose() {
            _adapter.Dispose();
        }
        #endregion

        #region [public]
        public string Invite() { return _adapter.SendRTCInvite(); }

        public string PUB_Sheet_NafiliPL(string What, string SetPL) { return _adapter.SendRTCMessage(new object[] { What, SetPL }); }

        public string PUB_Sheet_NapolniSheet(string What, string PLSet) { return _adapter.SendRTCMessage(new object[] { What, PLSet }); }

        public string PUB_Sifrant_NafiliPL(string What, string SetPL) { return _adapter.SendRTCMessage(new object[] { What, SetPL }); }
        
        public string PUB_Sifrant_NapolniSheet(string What, short nPage, string PLSet) { return _adapter.SendRTCMessage(new object[] { What, nPage, PLSet }); }

        public string PUB_POS_NapolniSheet(string Vrsta) { return _adapter.SendRTCMessage(new object[] { Vrsta }); }

        public string PUB_Birokrat_Get(string Vrsta, string par) { return _adapter.SendRTCMessage(new object[] { Vrsta, par }); }

        public string PUB_POS_Get(string Vrsta, string par) { return _adapter.SendRTCMessage(new object[] { Vrsta, par }); }

        public void PUB_Form_END(string What, bool DoUnload) { _adapter.SendRTCMessage(new object[] { What, DoUnload }); }

        public string PRIV_CRMToDoVnos_NafiliPL() { return _adapter.SendRTCMessage(new object[] { }); }

        public void HOTVpisScanDokumenta(string n1) { _adapter.SendRTCMessage(new object[] { n1 }); }

        public string BravoFppomTest() { return _adapter.SendRTCMessage(new object[] { }); }

        public string GetPL_FromControl(object n1, string KeyCategory) { return _adapter.SendRTCMessage(new object[] { n1, KeyCategory }); }

        public string PUB_ToolID_InterfaceDescription(int ID) { return _adapter.SendRTCMessage(new object[] { ID }); }
        #endregion

    }
}

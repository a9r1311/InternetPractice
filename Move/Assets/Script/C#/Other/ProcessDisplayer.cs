using UnityEngine;
using TMPro;

// ö‹Æ‚Å’ño‚·‚é‚½‚ß‚Ìˆ—‰Â‹‰»ƒNƒ‰ƒX
public sealed class ProcessDisplayer : MonoBehaviour
{
    public TMP_Text ProcessText;
    public TMP_Text PacketText;

    public void ChangeProcessText(string msg)
    {
        ProcessText.text = msg;
    }
    public void ChangePacketText(string msg)
    {
        PacketText.text = msg;
    }
}
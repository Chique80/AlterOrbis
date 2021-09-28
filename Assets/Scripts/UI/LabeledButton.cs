using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI 
{
    public class LabeledButton : Button
    {
        [SerializeField] Text m_Text;

        public string GetText()
        {
            if(m_Text != null)
            {
                return m_Text.text;
            }

            return null;
        }

        public void SetText(string text)
        {
            if(m_Text != null)
            {
                m_Text.text = text;
            }
        }
    }
}


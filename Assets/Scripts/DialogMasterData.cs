using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DialogMasterData
{
    public class DialogMasterDataClass<T>
    {
        public T[] Data;
    }

    [Serializable]
    public class CharacterData
    {
        public string Talker = default;

        public int Position = default;

        [HideInInspector]
        public string Messages = default;

        //[HideInInspector]
        public string AnimationType = default;

        [SerializeField,TextArea(0, 5)]
        string[] m_allMessages = default;

        public string[] AllMessages => m_allMessages;
        public void MessagesToArray()
        {
            string[] del = { "\n" };
            m_allMessages = Messages.Split(del, StringSplitOptions.None);
        }
    }

    [Serializable]
    public class ChoicesData
    {
        public int ChoicesNum;
    }
}

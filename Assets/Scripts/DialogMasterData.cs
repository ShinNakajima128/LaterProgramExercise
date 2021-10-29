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

        public string AnimationType = default;

        [HideInInspector]
        public string Messages = default;

        public int ChoicesId = default;

        public int NextId = default;

        [SerializeField,TextArea(0, 10)]
        string[] m_allMessages = default;

        public string[] AllMessages { get => m_allMessages; set => m_allMessages = value; }

        public void MessagesToArray()
        {
            string[] del = { "\n" };
            m_allMessages = Messages.Split(del, StringSplitOptions.None);
        }
    }

    [Serializable]
    public class ChoicesData
    {
        public int ChoicesId = default;

        [HideInInspector]
        public string Choices = default;

        [HideInInspector]
        public string NextMessageId = default;

        [SerializeField, TextArea(0, 10)]
        string[] m_allChoices = default;

        [SerializeField]
        int[] m_nextId = default;

        public string[] AllChoices { get => m_allChoices; set => m_allChoices = value; }
        public int[] NextId { get => m_nextId; set => m_nextId = value; }
        public void MessagesAndNextIdToArray()
        {
            string[] del = { "\n" };
            m_allChoices = Choices.Split(del, StringSplitOptions.None);
            var n = NextMessageId.Split(del, StringSplitOptions.None);
            m_nextId = new int[n.Length];
            for (int i = 0; i < n.Length; i++)
            {
                m_nextId[i] = int.Parse(n[i]);
            }
        }
    }
}

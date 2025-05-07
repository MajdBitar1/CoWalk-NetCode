﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace XPXR.Recorder.Models
{
    public class Question : RecordWithProperties
    {
        public string Label;
        public string Answer;
        public string SessionId;

        private Session Session;

        private List<QuestionProperty> _questionProperties = new List<QuestionProperty>();


        public Question(string label, string answer, Session session, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();

            this.Label = label;
            this.Answer = answer;
            this.Session = session;
            this.SessionId = session.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "Question";
        }

        public void AddQuestionProperty(string property, string value)
        {
            QuestionProperty questionProperty = new QuestionProperty(property, value, this);
            this._questionProperties.Add(questionProperty);
        }

        public void AddQuestionProperties(Dictionary<string,string> properties)
        {
            foreach (KeyValuePair<string,string> item in properties)
            {
                this.AddQuestionProperty(item.Key,item.Value);
            }
        }

        public List<QuestionProperty> GetQuestionProperties()
        {
            return this._questionProperties;
        }
        public override List<RecordBase> GetProperties()
        {
            return new List<RecordBase>(this._questionProperties);
        }

        public void UpdateAnswer(string answer)
        {
            this.Answer = answer;
        }
    }
}

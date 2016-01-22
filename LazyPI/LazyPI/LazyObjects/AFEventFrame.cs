﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyPI.LazyObjects
{
    public class AFEventFrame : BaseObject
    {
        private DateTimeOffset _StartTime;
        private DateTimeOffset _EndTime;
        private Lazy<AFElementTemplate> _Template;
        private Lazy<ObservableCollection<AFEventFrame>> _EventFrames;
        private Lazy<ObservableCollection<AFAttribute>> _Attributes;
        private ObservableCollection<string> _CategoryNames;
        private static IAFEventFrame _EventFrameLoader;

        #region "Properties"
        public DateTimeOffset StartTime
        {
            get
            {
                return _StartTime;
            }
            set
            {
                _StartTime = value;
            }
        }

        public DateTimeOffset EndTime
        {
            get
            {
                return _EndTime;
            }
            set
            {
                _EndTime = value;
            }
        }

        public AFElementTemplate Template
        {
            get
            {
                return _Template.Value;
            }
        }

        public IEnumerable<string> CategoryNames
        {
            get
            {
                return _CategoryNames;
            }
        }
        #endregion

        #region "Constructors"
            private AFEventFrame(string ID, string Name, string Description, string Path) 
                : base(ID, Name, Description, Path)
            {
                Initialize();
            }

            private void Initialize()
            {
                _Template = new Lazy<AFElementTemplate>(() =>
                {
                   var templateName = _EventFrameLoader.GetEventFrameTemplate(this._ID);
                   return AFElementTemplate.FindByName(templateName);
                }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

                _EventFrames = new Lazy<ObservableCollection<AFEventFrame>>(() => {
                    var frames = _EventFrameLoader.GetChildFrames(_ID, SearchMode.None, "*-8d", "*", "*", "*", "*", "*", "*", false, "Name", "Ascending", 0, 1000);

                    ObservableCollection<AFEventFrame> obsList = new ObservableCollection<AFEventFrame>(EventFrameFactory.CreateInstanceList(frames));
                    obsList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ChildrenChanged);
                    
                    return obsList;
                }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

                _Attributes = new Lazy<ObservableCollection<AFAttribute>>(() => { 
                    var attrs = _EventFrameLoader.GetAttributes(this._ID, "*", "*", "*", "*", false, "Name", "Ascending", 0, false, false, 1000);
                    ObservableCollection<AFAttribute> obsList = new ObservableCollection<AFAttribute>();

                    foreach (var attribute in attrs)
                    {
                        obsList.Add(AFAttribute.Find(attribute.ID));
                    }

                    obsList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AttributesChanged);

                    return obsList;
                }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            }
        #endregion

        #region "Callbacks"
        private void AttributesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                AFAttribute.Create(this._ID, (AFAttribute)sender);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                AFAttribute obj = (AFAttribute)sender;
                AFAttribute.Delete(obj.ID);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                throw new NotImplementedException("Replace is not supported by LazyPI.");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                throw new NotImplementedException("Reset is not supported by LazyPI.");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                throw new NotImplementedException("Move is not supported by LazyPI.");
            }
        }

        private void ChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                _EventFrameLoader.CreateEventFrame(this._ID, (AFEventFrame)sender);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                AFEventFrame obj = (AFEventFrame)sender;
                _EventFrameLoader.Delete(obj._ID);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                throw new NotImplementedException("Replace is not supported by LazyPI.");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                throw new NotImplementedException("Reset is not supported by LazyPI.");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                throw new NotImplementedException("Move is not supported by LazyPI.");
            }
        }
        #endregion

        public class EventFrameFactory
        {
            public static AFEventFrame CreateInstance(string ID, string Name, string Description, string Path)
            {
                return new AFEventFrame(ID, Name, Description, Path);
            }

            public static List<AFEventFrame> CreateInstanceList(IEnumerable<BaseObject> frames)
            {
                List<AFEventFrame> results = new List<AFEventFrame>(); 

                foreach (var baseFrame in frames)
                {
                    results.Add(new AFEventFrame(baseFrame.ID, baseFrame.Name, baseFrame.Description, baseFrame.Path));
                }

                return results;
            }
        }
    }
}

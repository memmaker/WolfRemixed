﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;

namespace Twengine.Components
{
    public class Inventory : Component
    {
        public Entity[] Items { get; set; }
        private int mCapacity;
        private int mSelectedItem;
        public int SelectedItem
        {
            get { return mSelectedItem; }
            set 
            { 
                mSelectedItem = value >= 0 && value <= mCapacity - 1 ? value : -1; 
            }
        }
        public Inventory(int capacity)
        {
            Items = new Entity[capacity];
            mCapacity = capacity;
            SelectedItem = -1;
        }
        public Entity GetSelectedItem()
        {
            if (mSelectedItem == -1) throw new IndexOutOfRangeException("No item selected..");
            return Items[mSelectedItem];
        }
        
    }
}

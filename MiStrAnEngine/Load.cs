﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiStrAnEngine
{
    public class Load
    {
        public Node node;

        // Field variables
        public Vector loadVec;


        //Constructor
        public Load(Node _node, Vector _loadVec)
        {
            node = _node;
            loadVec = _loadVec;
        }

    }
}

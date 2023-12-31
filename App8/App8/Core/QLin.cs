﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public struct QLin
    {
        public bool Init(string as_head, string as_body)
        { 
            if (as_head.Length == 0)
        	{
		        return false;
	        }
	
	        

	        string[] l_listHead = as_head.Split(new char[]{','});
	        if (l_listHead.Length < 1)
	        {
		        return false;
	        }

	        m_name = l_listHead[0].Trim();
	        if (l_listHead.Length > 1) //desc not mandatory
	        {
		        m_desc = l_listHead[1];
	        }
	        m_body = as_body;


	        return true;
        }

        public string m_name;
        public string m_desc;
        public string m_body;
    }
}

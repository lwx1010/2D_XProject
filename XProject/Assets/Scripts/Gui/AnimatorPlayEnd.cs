using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AnimatorPlayEnd : MonoBehaviour
{
   public bool isPlaying { get; set; }

    void Awake()
    {
        isPlaying = true;
    }

    void Start()
    {
        
    }

    public void PlayEnd()
    {
        isPlaying = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSelector : MonoBehaviour {

    public int[] TriggerNames;
    public Animator characteranim;
    private int i = 0;
	public void Previous()
    {

        if(i>0)
        {
            i--;
            characteranim.SetTrigger(TriggerNames[i].ToString());
            


        }

    }

    public void Next()
    {
        if (i < 6)
        {
            i++;
            characteranim.SetTrigger(TriggerNames[i].ToString());



        }

    }

}

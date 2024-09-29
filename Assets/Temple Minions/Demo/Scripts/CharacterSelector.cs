using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelector : MonoBehaviour {

    public GameObject CharacterFirst;
    public GameObject CharacterSecond;
    public GameObject CharacterThird;

    public void SelectFirst()
    {

        CharacterFirst.SetActive(true);
        CharacterSecond.SetActive(false);
        CharacterThird.SetActive(false);

    }


    public void SelectSecond()
    {

        CharacterFirst.SetActive(false);
        CharacterSecond.SetActive(true);
        CharacterThird.SetActive(false);

    }

    public void SelectThird()
    {

        CharacterFirst.SetActive(false);
        CharacterSecond.SetActive(false);
        CharacterThird.SetActive(true);

    }

}

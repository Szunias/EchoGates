using NUnit.Framework.Constraints;
using System;
using UnityEngine;

public class FarmMechanic : MonoBehaviour
{
    [SerializeField] private float m_timeToGrow; //The time for a crop to grow, can be later changed to the day-night cycle internal clock
    [SerializeField] private float m_wateringMultiplier;

    private bool startGrowing = false; //Bool to check if plant is growing
    private bool growingComplete = false; //Bool to check if plant growth is complete
    private float processOfGrowth = 0; //Float to check how much the plant has grown

    private void Start() //Temporary file to start the script for planting
    {
        gameObject.transform.localPosition = Vector3.zero;
    }

    private void Plant() //Function that detects when planting
    {
        if (startGrowing == false) //Check to see that the planting process is running already
        {
            startGrowing = true;
            gameObject.transform.localPosition = new Vector3(0, 0.2f, 0);
            growingComplete = false;
        }
    }

    private void Update() //Growing phrase
    {
        if (Input.GetKeyDown("p")) //When "P" is pressed, planting will begin
        {
            Plant();
        }

        if (Input.GetKeyDown("h")) //When "H" is pressed, plant will be harvested
        {
            if (growingComplete)
            {
                Debug.Log("Plant sucessfully harvested");
                gameObject.transform.localPosition = Vector3.zero;
                startGrowing = false;
            }
        }

        if (Input.GetKeyDown("w")) //When "W" is pressed, it will water the plant and make it grow faster
        {
            float rand = UnityEngine.Random.Range(0.1f, 0.2f); //random amount of effect added on thanks to watering
            m_wateringMultiplier += rand;
            if (m_wateringMultiplier > 1.1f)
            {
                m_wateringMultiplier = 1.0f;
            }
        }

        if (startGrowing) //The growing process
        {
            if (!growingComplete)
            {
                float timeToGrow = m_timeToGrow;
                float growPosition = processOfGrowth * 0.1f;
                float watering = m_wateringMultiplier;
                gameObject.transform.localPosition = new Vector3(0, 0.2f * watering - growPosition, 0); //0.2 is the starting position minus the growPosition to make it go up
                processOfGrowth -= 1.0f / timeToGrow;
            }
        }

        if (gameObject.transform.position.y > 0.5f) //Once reaching 0.5 on y, the plant is considered "fully farmed"
        {
            processOfGrowth = 0;
            growingComplete = true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField]private int _remainingCaptures = 5;

    private Dictionary<ETeam, int> _teamsCaptures = new Dictionary<ETeam, int>();
    
    private float _timeToCapture = 1f;
    private ETeam _teamOwner = ETeam.NONE;
    private List<CarController> _allCars = new List<CarController>();

    private void Awake()
    {
        _teamsCaptures.Add(ETeam.RED, _remainingCaptures);
        _teamsCaptures.Add(ETeam.BLUE, _remainingCaptures);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CarController>())
        {
            CarController carCapture = other.GetComponent<CarController>();
            _allCars.Add(carCapture);
            
            if (_teamOwner != carCapture.MyTeam && _allCars.Count <= 1)
            {

                StartCoroutine(CaptureTerritories(carCapture.MyTeam));
            }
            
            else if (_allCars.Count > 1)
            {
                StopAllCoroutines();
            }

        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CarController>())
        {
            CarController carCapture = other.GetComponent<CarController>();
            _allCars.Remove(carCapture);
            
            if (other.tag == "Player" && _teamOwner != carCapture.MyTeam)
            {
                Debug.Log("Exited PlatformController");
                
                StopCoroutine(CaptureTerritories(carCapture.MyTeam));
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
    }
    
    private IEnumerator CaptureTerritories(ETeam team)
    {
        yield return new WaitForSeconds(_timeToCapture);
        if (_teamsCaptures[team] <= 0)
        {
            _teamOwner = team;
            _teamsCaptures[team] = _remainingCaptures;
            
        }
        else
        {
            _teamsCaptures[team]--;
            StartCoroutine(CaptureTerritories(team));
        }
        
    }


}

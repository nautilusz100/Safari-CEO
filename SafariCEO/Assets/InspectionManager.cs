using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;

public class InspectionManager : MonoBehaviour
{

    public TMP_Text title;
    public GameObject selected = null;


    public GameObject jeepSelect;
    public TMP_Text passengersText;

    public GameObject animalSelect;
    public TMP_Text animalStats;
    public Image animalImage;

    public GameObject natureSelect;
    public TMP_Text natureStats;
    public Image natureImage;
    
    public GameObject window;

    public GameManager gameManager;
    public CameraManager cameraManager;


    


    void Start()
    {
        
    }
    private void OnClick(RaycastHit2D hit)
    {
        string title = hit.transform.gameObject.name;

        var match = Regex.Match(title, @"tile_x(\d+)_y(\d+)");

        if (match.Success)
        {
            int x = int.Parse(match.Groups[1].Value);
            int y = int.Parse(match.Groups[2].Value);

            title = $"Tile (x: {x}, y: {y})";
        }
        Display(title, hit.transform.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        UpdateStats();

        if (UnityEngine.Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("Tiles"));
            if (hit.collider != null)
            {
                OnClick(hit);
            }
        }
    }


    public void CloseWindow()
    {
        window.SetActive(false);
    }

    public void Focus()
    {
        if (selected != null)
        {
            cameraManager.JumpTo(selected.transform.position);
        }
    }

    public void Sell()
    {
        if (selected != null)
        {
            Animal anim = selected.GetComponent<Animal>();
            if (anim != null)
            {
                gameManager.Money += 100;
                Destroy(selected);
                selected = null;
                CloseWindow();

            }
        }
    }

    public void Display(int tourists, int id)
    {
        title.text = "Jeep "+id.ToString();
        passengersText.text = "Passengers: "+tourists.ToString();
        natureSelect.SetActive(false);
        animalSelect.SetActive(false);
        
        window.SetActive(true);
        jeepSelect.SetActive(true);
        selected = null;
    }

    public void UpdateStats()
    {
        if (selected != null)
        {
            Animal anim = selected.GetComponent<Animal>(); 
            Tile tile = selected.GetComponent<Tile>();
            if (tile != null)
            {
                natureStats.text = $"Type: {tile.Type}\nFood Value: {tile.FoodAmount}";
                
                if (natureImage.sprite != selected.GetComponent<SpriteRenderer>().sprite)
                {
                    natureImage.sprite = selected.GetComponent<SpriteRenderer>().sprite;
                }

            }
            else if (anim != null)
            {

                int age;
                int thirst;
                int hunger;
                string state;
                string diet;

                if (anim.diet == Animal.Diet.Carnivore)
                {
                    Carnivorous carn = selected.GetComponent<Carnivorous>();
                    diet = carn.diet.ToString();
                    age = Mathf.FloorToInt(carn.age / 100);
                    hunger = Mathf.RoundToInt((1 - carn.hungerTimer / carn.hungerInterval)*100);
                    thirst = Mathf.RoundToInt((1 - carn.thirstTimer / carn.thirstInterval) * 100);
                    state = carn.CurrentState.ToString();
                    state = Regex.Replace(state, @"([a-z])([A-Z])", "$1 $2");

                }
                else
                {
                    Herbivore herb = selected.GetComponent<Herbivore>();
                    diet = herb.diet.ToString();
                    age = Mathf.FloorToInt(herb.age / 100);
                    hunger = Mathf.RoundToInt((1 - herb.hungerTimer / herb.hungerInterval)*100);
                    thirst = Mathf.RoundToInt((1 - herb.thirstTimer / herb.thirstInterval)*100);
                    state = herb.CurrentState.ToString();
                    state = Regex.Replace(state, @"([a-z])([A-Z])", "$1 $2");
                }
                
                animalStats.text = $"Diet: {diet}\nAge: {age} yrs\nHunger: {hunger}%\nThirst: {thirst}%\nState: {state}";
            }


        }
    }

    public void Display(GameObject select)
    {
        natureSelect.SetActive(false);
        jeepSelect.SetActive(false);

        window.SetActive(true);
        animalSelect.SetActive(true);

        title.text = select.name.Replace("(Clone)","");
        selected = select;
        animalImage.sprite = selected.GetComponent<SpriteRenderer>().sprite;
        UpdateStats();
    }

    public void Display(string title, GameObject select)
    {

        animalSelect.SetActive(false);
        jeepSelect.SetActive(false);

        window.SetActive(true);
        natureSelect.SetActive(true);
        this.title.text = title;
        Tile tile = select.GetComponent<Tile>();
        natureStats.text = $"Type: {tile.Type}\nFood Value: {tile.FoodAmount}";
        natureImage.sprite = select.GetComponent<SpriteRenderer>().sprite;
        selected = select;
    }
}

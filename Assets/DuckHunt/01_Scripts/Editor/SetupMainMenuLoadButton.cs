using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class SetupMainMenuLoadButton
{
    [MenuItem("Duck Hunt VR/Configurar Boton Cargar en Menu", false, 60)]
    public static void ConfigurarBoton()
    {
        string scenePath = "Assets/DuckHunt/00_Scene/MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        MainMenuManager menuMgr = Object.FindAnyObjectByType<MainMenuManager>();
        if (menuMgr == null)
        {
            Debug.LogError("[SetupMainMenu] No se encontró MainMenuManager en la escena MainMenu.");
            return;
        }

        // Buscar Contenedor_Botones
        GameObject contenedor = GameObject.Find("Contenedor_Botones");
        Transform parentTransform = contenedor != null ? contenedor.transform : null;

        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Button botonJugar = null;
        Button botonSalir = null;
        Button botonCargar = null;

        foreach (var b in buttons)
        {
            string nameLower = b.gameObject.name.ToLower();
            if (nameLower.Contains("cargar") || nameLower.Contains("load") || nameLower.Contains("continuar"))
            {
                botonCargar = b;
            }
            else if (nameLower.Contains("jugar") || nameLower.Contains("play") || nameLower.Contains("start"))
            {
                botonJugar = b;
            }
            else if (nameLower.Contains("salir") || nameLower.Contains("quit") || nameLower.Contains("exit"))
            {
                botonSalir = b;
            }
        }

        if (botonCargar == null && botonJugar != null)
        {
            GameObject newBtnObj = Object.Instantiate(botonJugar.gameObject, botonJugar.transform.parent);
            newBtnObj.name = "Boton_CargarPartida";
            botonCargar = newBtnObj.GetComponent<Button>();
        }

        if (botonCargar != null)
        {
            // Poner el botón activo
            botonCargar.gameObject.SetActive(true);
            botonCargar.interactable = true;

            // Ponerlo justo después de Boton_Jugar
            if (botonJugar != null)
            {
                int jugarIndex = botonJugar.transform.GetSiblingIndex();
                botonCargar.transform.SetSiblingIndex(jugarIndex + 1);
            }

            // Cambiar color para que destaque (Azul / Cyan atractivo)
            Image img = botonCargar.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.15f, 0.45f, 0.85f, 1f); // Azul
            }

            var colors = botonCargar.colors;
            colors.normalColor = new Color(0.15f, 0.45f, 0.85f, 1f);
            colors.highlightedColor = new Color(0.25f, 0.6f, 1f, 1f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.65f, 1f);
            colors.selectedColor = new Color(0.2f, 0.5f, 0.9f, 1f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            botonCargar.colors = colors;

            // Configurar texto del botón
            TextMeshProUGUI tmp = botonCargar.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "CARGAR PARTIDA";
                tmp.color = Color.white;
            }
            else
            {
                Text txt = botonCargar.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = "CARGAR PARTIDA";
                    txt.color = Color.white;
                }
            }

            // Asignar evento OnClick -> MainMenuManager.CargarPartida
            botonCargar.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(botonCargar.onClick, menuMgr.CargarPartida);

            menuMgr.botonCargarPartida = botonCargar.gameObject;
        }

        EditorUtility.SetDirty(menuMgr);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupMainMenu] ¡Botón 'CARGAR PARTIDA' reordenado, coloreado y guardado en MainMenu.unity!");
    }
}

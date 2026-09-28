using UnityEngine;

public class FullscreenToggle : MonoBehaviour
{
    [SerializeField] KeyCode tecla = KeyCode.F11;

    // Guarda o tamanho da janela antes de ir para tela cheia,
    // para conseguir voltar exatamente para ele.
    int larguraJanela = 1280;
    int alturaJanela = 720;

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (Input.GetKeyDown(tecla)) Alternar();
#endif
    }

    void Alternar()
    {
        if (Screen.fullScreen)
        {
            Screen.SetResolution(larguraJanela, alturaJanela, FullScreenMode.Windowed);
        }
        else
        {
            larguraJanela = Screen.width;
            alturaJanela = Screen.height;

            Screen.SetResolution(
                Display.main.systemWidth,
                Display.main.systemHeight,
                FullScreenMode.FullScreenWindow);
        }
    }
}
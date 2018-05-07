using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
    public class StartupDialogue : EventsSubscriber
    {
        public AppearanceManager MessagePanel;
        public float MessagePanelTimeout = 3;
        public AppearanceManager QuestionPanel;



        public Button ButtonYes;
        public Button ButtonNo;

        // Use this for initialization
        void Start()
        {
            ButtonYes.onClick.AddListener(OnButtonYesClicked); ;
            ButtonNo.onClick.AddListener(OnButtonNoClicked);
            MessagePanel.gameObject.SetActive(false);
            QuestionPanel.gameObject.SetActive(false);
        }

        void ShowMessagePanel(float timeout = 1)
        {
            Debug.Log("ShowMessagePanel");
             StartCoroutine
            (
               ShowMessagePanelCoroutine(timeout)
            );
        }

        IEnumerator ShowMessagePanelCoroutine(float timeout)
        {
            yield return null;
            yield return StartCoroutine
                ( ShowPanelCoroutine(MessagePanel, timeout));
            Debug.Log("--------   panel is shown ------");
            yield return new WaitForSeconds(MessagePanelTimeout);
            MessagePanel.Hide();


        }

        public void ShowQuestionPanel()
        {
            Debug.Log("ShowQuestionPanel");
            StartCoroutine
           (
               ShowPanelCoroutine(QuestionPanel, 0.5f)
           );
        }


        public void HideQuestionPanel()
        {
            Debug.Log("HideQuestionPanel");
            QuestionPanel.Hide();

        }

        IEnumerator ShowPanelCoroutine(AppearanceManager panel, float timeout)
        {
            yield return null;
            Debug.Log("Started coroutine ShowPanelCoroutine for " + panel.gameObject.name + " timeout " + timeout);
            if (timeout > 0.25)
                yield return new WaitForSeconds(timeout);
            panel.gameObject.SetActive(true);
            yield return new WaitUntil(() => panel.IsReady);
             panel.Show();


        }

        bool DoOnce = true;
        protected override void Update()
        {
            base.Update();
            if (DoOnce)
            {
                DoOnce = false;
                ShowMessagePanel(3);
            }
        }


        protected override void NotifySomethingHappened(GameData.SomethingId id)
        {
            if (id == GameData.SomethingId.WorldRootPlaced)
            {
                Debug.Log("Got the WorldRootMoved event ");
                ShowQuestionPanel();
            }
            else if (id == GameData.SomethingId.GameStart)
            {
                QuestionPanel.gameObject.SetActive(false);
                MessagePanel.gameObject.SetActive(false);

            }


        }


        void OnButtonYesClicked()
        {
            //GameController.StartGame();
            QuestionPanel.Hide();
        }

        void OnButtonNoClicked()
        {
            //GameController.StartGame();
            ShowMessagePanel(0.5f);
        }
    }
}

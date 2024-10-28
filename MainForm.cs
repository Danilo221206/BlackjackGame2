using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;

namespace BlackjackGame2
{
    public partial class MainForm : Form
    {
        // Definir vari�veis do jogador, dealer e o valor da aposta
        private Player player;
        private SoundPlayer drawcardsfx;
        private SoundPlayer shufflesfx;
        private SoundPlayer hitsfx;
        private Dealer dealer;
        private static readonly HttpClient client = new HttpClient();
        private string deckId;
        private bool betdone = false;
        int action = 0;

        public MainForm()
        {
            InitializeComponent();
            player = new Player { Balance = 1000 }; // Exemplo de saldo inicial
            dealer = new Dealer();
            hitsfx = new SoundPlayer("C:\\Users\\danil\\source\\repos\\BlackjackGame2\\BlackjackGame2\\hit.sfx.wav");
            shufflesfx = new SoundPlayer("C:\\Users\\danil\\source\\repos\\BlackjackGame2\\BlackjackGame2\\shufflecards.sfx.wav");
            Dinheiro.Value = 1000;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        // Inicia uma nova rodada
        private async void StartNewGame()
        {
            Playerscore.Text = "Sua pontua��o:";
            Dealerscore.Text = "Pontua��o do dealer:";

            deckId = await CreateDeck();
            cartadealer1.Visible = false; cartadealer2.Visible = false; cartadealer3.Visible = false; cartadealer4.Visible = false;
            cartaplayer1.Visible = false; cartaplayer2.Visible = false; cartaplayer3.Visible = false; cartaplayer4.Visible = false; cartaplayer5.Visible = false;
            if (betdone == false)
            {
                MessageBox.Show("Novo jogo! Fa�a suas apostas antes de come�ar.");
                Aposta.Enabled = true;
                Hitbtn.Enabled = false;
                Standbtn.Enabled = false;
                Doublebtn.Enabled = false;

            }
            else await DealInitialCards();
        }

        // Cria um novo baralho embaralhado
        private async Task<string> CreateDeck()
        {
            var response = await client.GetStringAsync("https://deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1");
            dynamic deck = JsonConvert.DeserializeObject(response);
            return deck.deck_id;
        }

        // Distribui as cartas iniciais (2 para o jogador e 2 para o dealer)
        private async Task DealInitialCards()
        {
            drawcardsfx = new SoundPlayer("C:\\Users\\danil\\source\\repos\\BlackjackGame2\\BlackjackGame2\\drawcard.sfx.wav");
            Hitbtn.Enabled = true;
            Standbtn.Enabled = true;
            Doublebtn.Enabled = true;
            action = 0;
            player.PlayerHand.Cards.Clear();
            dealer.DealerHand.Cards.Clear();
            ScoreManager scoreManager = new ScoreManager();
            player.PlayerHand.Cards.Add(await DrawCard()); cartaplayer1.Visible = true; drawcardsfx.Play(); Thread.Sleep(1000);
            dealer.DealerHand.Cards.Add(await DrawCard()); cartadealer1.Visible = true; drawcardsfx.Play(); Thread.Sleep(1000);
            player.PlayerHand.Cards.Add(await DrawCard()); cartaplayer2.Visible = true; drawcardsfx.Play(); Thread.Sleep(1000);
            dealer.DealerHand.Cards.Add(await DrawCard()); cartadealer2.Visible = true; drawcardsfx.Play(); Thread.Sleep(1000);
            cartaplayer1.Load(player.PlayerHand.Cards[0].ImageUrl);
            cartaplayer2.Load(player.PlayerHand.Cards[1].ImageUrl);
            cartadealer1.Load(dealer.DealerHand.Cards[0].ImageUrl);
            //int dealerScore = scoreManager.CalculateHandValue(dealer.DealerHand.Cards);
            Dealerscore.Text = $"Pontua��o do dealer: ?";
            cartadealer2.Load(dealer.DealerHand.Cards[1].ImageUrl);
            int playerScore = scoreManager.CalculateHandValue(player.PlayerHand.Cards);
            Playerscore.Text = $"Sua pontua��o: {playerScore}";
            int dealerScore = scoreManager.CalculateHandValue(dealer.DealerHand.Cards);
            Dealerscore.Text = $"Pontua��o do dealer: {dealerScore}";
            if (playerScore == 21)
            {
                MessageBox.Show("BLACKJACK!! Voc� ganhou");
                Dinheiro.Value += Aposta.Value * 3 / 2;
                betdone = false;
                Aposta.Value = 0;
                StartNewGame();
            }

            // Dealer recebe a primeira carta (virada para cima)

        }

        // Saca uma carta do baralho da API
        private async Task<Card> DrawCard()
        {
            string url = $"https://deckofcardsapi.com/api/deck/{deckId}/draw/?count=1";
            var response = await client.GetStringAsync(url);
            dynamic draw = JsonConvert.DeserializeObject(response);

            return new Card
            {
                Value = draw.cards[0].value,
                Suit = draw.cards[0].suit,
                ImageUrl = draw.cards[0].image
            };
        }
        // Jogador escolhe 'Hit' (pedir mais uma carta)
        private async void Hitbtn_Click(object sender, EventArgs e)
        {
            ScoreManager scoreManager = new ScoreManager();
            hitsfx.Play();
            Thread.Sleep(1000);
            drawcardsfx.Play();
            // O jogador recebe uma nova carta e ela � adicionada � m�o
            player.PlayerHand.Cards.Add(await DrawCard());
            drawcardsfx.Play();
            // Atualiza a imagem da nova carta (supondo que seja cartaplayer3, por exemplo)
            if (action == 0)
            {
                cartaplayer3.Visible = true;
                cartaplayer3.Load(player.PlayerHand.Cards[2].ImageUrl);
                action++;
            }
            else if (action == 1)
            {
                cartaplayer4.Visible = true;
                cartaplayer4.Load(player.PlayerHand.Cards[3].ImageUrl);
                action++;
            }
            else if (action == 2)
            {
                cartaplayer5.Visible = true;
                cartaplayer5.Load(player.PlayerHand.Cards[4].ImageUrl);
                action++;
            }

            // Calcula e exibe a nova pontua��o do jogador
            int playerScore = scoreManager.CalculateHandValue(player.PlayerHand.Cards);
            Playerscore.Text = $"Sua pontua��o: {playerScore}";
            Thread.Sleep(2500);
            // Verifica se o jogador estourou (bust)
            if (player.PlayerHand.IsBust())
            {
                MessageBox.Show("Voc� perdeu! A m�o passou de 21.");
                betdone = false;
                Aposta.Value = 0;
                StartNewGame();
            }
        }

        // Jogador escolhe 'Stand' (ficar com as cartas)
        // Jogador escolhe 'Stand' (ficar com as cartas)
        private async void Standbtn_Click(object sender, EventArgs e)
        {
            await dealer.Play(DrawCard, this); // Passa o formul�rio como par�metro
            CheckWinner();
        }


        // Verifica o vencedor ap�s o turno do dealer
        private async void CheckWinner()
        {
            if (player.PlayerHand.GetTotalValue() > dealer.DealerHand.GetTotalValue() || dealer.DealerHand.GetTotalValue() > 21)
            {
                MessageBox.Show("Voc� ganhou!");
                Dinheiro.Value += Aposta.Value * 2; // Jogador ganha o dobro da aposta
            }
            else if(player.PlayerHand.GetTotalValue() == dealer.DealerHand.GetTotalValue())
            {
                MessageBox.Show("O jogo empatou");
                Dinheiro.Value += Aposta.Value;
            }
            else{
                MessageBox.Show("O dealer ganhou!");
            }
            betdone = false;
            Aposta.Value = 0;
            StartNewGame();

        }
        private void button4_Click(object sender, EventArgs e)
        {
            //Betbtn!! nome bugoukkkk
            if (Aposta.Value > 0)
            {
                if (betdone == false)
                {
                    Aposta.Enabled = false;
                    decimal valor = (Dinheiro.Value - Aposta.Value);
                    if ((valor) < 0) MessageBox.Show("Voc� n�o possui dinheiro para isso");
                    else
                    {
                        Dinheiro.Value -= Aposta.Value;
                        StartNewGame();
                    }
                }
                else
                {
                    MessageBox.Show("Sua aposta n�o pode ser alterada at� o final da partida!");
                }
                betdone = true;
            }
            else MessageBox.Show("Insira um valor v�lido.");
        }



        // Representa uma carta
        public class Card
        {
            public string Value { get; set; }
            public string Suit { get; set; }
            public string ImageUrl { get; set; }

            public int GetCardValue()
            {
                if (Value == "ACE") return 11;
                if (Value == "KING" || Value == "QUEEN" || Value == "JACK") return 10;
                return int.TryParse(Value, out int result) ? result : 0;
            }
        }

        // Representa uma m�o (cartas de um jogador ou dealer)
        public class Hand
        {
            public List<Card> Cards { get; set; } = new List<Card>();

            public int GetTotalValue()
            {
                int totalValue = Cards.Sum(card => card.GetCardValue());
                int aceCount = Cards.Count(card => card.Value == "ACE");

                // Ajusta o valor do �s de 11 para 1 se necess�rio
                while (totalValue > 21 && aceCount > 0)
                {
                    totalValue -= 10;
                    aceCount--;
                }

                return totalValue;
            }

            public bool IsBlackjack() => GetTotalValue() == 21 && Cards.Count == 2;
            public bool IsBust() => GetTotalValue() > 21;
        }

        // Representa o Dealer
        public class Dealer
        {
            public Hand DealerHand { get; set; } = new Hand();
            private SoundPlayer drawcardsfx;

            // Dealer joga at� atingir 17 ou mais
            // Dealer joga at� atingir 17 ou mais
            public async Task Play(Func<Task<Card>> drawCardFunc, MainForm form)
            {
                drawcardsfx = new SoundPlayer("C:\\Users\\danil\\source\\repos\\BlackjackGame2\\BlackjackGame2\\drawcard.sfx.wav");
                while (DealerHand.GetTotalValue() < 17)
                {

                    // O dealer saca uma nova carta usando a fun��o fornecida
                    Card newCard = await drawCardFunc();
                    drawcardsfx.Play();
                    Thread.Sleep(1000);
                    DealerHand.Cards.Add(newCard);

                    // Atualiza a interface gr�fica
                    if (!form.cartadealer3.Visible)
                    {
                        form.cartadealer3.Visible = true;
                        form.cartadealer3.Load(newCard.ImageUrl);
                    }
                    else if (!form.cartadealer4.Visible)
                    {
                        form.cartadealer4.Visible = true;
                        form.cartadealer4.Load(newCard.ImageUrl);
                    }
                }
            }


            // Simular carta do dealer (ajustar com a API)
            private Card DrawCardFromDeck()
            {
                // Este m�todo deve ser implementado para funcionar corretamente
                return new Card(); // Exemplo
            }
        }

        // Representa o jogador
        public class Player
        {
            public Hand PlayerHand { get; set; } = new Hand();
            public decimal Balance { get; set; }

            public void PlaceBet(decimal amount)
            {
                Balance -= amount;
            }

            public void Hit(Card card)
            {
                PlayerHand.Cards.Add(card);
            }

            public bool CanDouble() => PlayerHand.Cards.Count == 2;

            // Dobrar aposta
            public void DoubleBet(decimal originalBet)
            {
                Balance -= originalBet; // Dobrar a aposta original
            }
        }
        public class ScoreManager
        {
            public int CalculateHandValue(List<Card> hand)
            {
                int totalValue = 0;
                int aceCount = 0;

                foreach (var card in hand)
                {
                    if (int.TryParse(card.Value, out int cardValue))
                    {
                        // Cartas num�ricas (2-10)
                        totalValue += cardValue;
                    }
                    else if (card.Value == "JACK" || card.Value == "QUEEN" || card.Value == "KING")
                    {
                        // Cartas de face valem 10
                        totalValue += 10;
                    }
                    else if (card.Value == "ACE")
                    {
                        // �s pode valer 1 ou 11
                        aceCount++;
                        totalValue += 11; // Inicialmente, considere o �s como 11
                    }

                }

                // Ajusta os �ses para 1 se a pontua��o exceder 21
                while (totalValue > 21 && aceCount > 0)
                {
                    totalValue -= 10; // Considera o �s como 1
                    aceCount--;
                }

                return totalValue;
            }
        }

        private void Doublebtn_Click(object sender, EventArgs e)
        {
            if (Dinheiro.Value / 2 < Aposta.Value)
            {
                MessageBox.Show("voc� n�o tem dinheiro pra isso");
                Doublebtn.Enabled = false;
            }
            else
            {
                Dinheiro.Value -= Aposta.Value;
                Aposta.Value = Aposta.Value * 2;
                Hitbtn_Click(sender, e);
            }


        }

        private void MainForm_Load_1(object sender, EventArgs e)
        {
            Aposta.Enabled = true;
            Hitbtn.Enabled = false;
            Standbtn.Enabled = false;
            Doublebtn.Enabled = false;
        }
    }
}
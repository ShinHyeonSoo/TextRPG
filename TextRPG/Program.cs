using System.Numerics;
using static TextRPG.Program;
using Newtonsoft.Json;
using System.IO;

namespace TextRPG
{
    internal class Program
    {
        #region Interface
        public interface ICharacter
        {
            string Name{ get; set; }
            int Health { get; set; }
            float Attack {  get; set; }
            bool IsDead { get; set; }

            void TakeDamage(int damage);
        }

        public interface IItem
        {
            string Name { get; set; }
            public void Use(Warrior warrior);
        }
        #endregion

        #region Struct, Enum
        public struct ItemInfo
        {
            public string? name;
            public string? text;
            public int price;
            public bool isBuy;

            public ItemInfo()
            {
                isBuy = false;
            }
        }

        public enum EquipSlot
        {
            WEAPON,
            ARMOR,
            NONE
        }

        public enum Dungeon 
        {
            EASY = 5,
            NORMAL = 11,
            HARD = 17,
            NONE
        }
        #endregion

        #region Character
        public class Warrior : ICharacter
        {
            private string? name;
            private int level;
            private int health;
            private float attack;
            private int itemAttack;
            private int def;
            private int itemDef;
            private int gold;
            private bool isDead;

            private List<Equip> myEquips = new List<Equip>();
            private Equip[] equipSlots = new Equip[(int)EquipSlot.NONE];

            public string Name { get { return name ?? "Unknown"; } set { name = value; } }
            public int Level { get { return level; } set { level = value; } }
            public int Health { get { return health; } set { health = value; } }
            public float Attack { get { return attack; } set { attack = value; } }
            public int ItemAttack { get { return itemAttack; } set { itemAttack = value; } }
            public int Def { get { return def; } set { def = value; } }
            public int ItemDef { get { return itemDef; } set { itemDef = value; } }
            public int Gold { get { return gold; } set { gold = value; } }
            public bool IsDead { get { return isDead; } set { isDead = value; } }
            public List<Equip> MyEquips { get { return myEquips; } }
            public Equip[] EquipSlots { get { return equipSlots; } }

            public void TakeDamage(int damage)
            {
                health -= damage;
            }

            public void TakeRest(int useGold)
            {
                health = 100;
                gold -= useGold;
            }

            public void LevelUp()
            {
                attack += 0.5f;
                def += 1;
                ++level;
            }

            public void BuyItem(Equip item)
            {
                myEquips.Add(item);
                gold -= item.Price;
                item.IsBuy = true;
            }

            public void SellItem(Equip item)
            {
                myEquips.Remove(item);
                gold += (int)(item.Price * 0.85f);
                item.IsBuy = false;
            }

            public void EquipItem(Equip item)
            {
                if(item.Attack != -1)
                {
                    if (equipSlots[(int)EquipSlot.WEAPON] != null)
                    {
                        UnEquipItem(equipSlots[(int)EquipSlot.WEAPON]);
                    }

                    equipSlots[(int)EquipSlot.WEAPON] = item;
                    itemAttack += item.Attack;
                }
                else
                {
                    if (equipSlots[(int)EquipSlot.ARMOR] != null)
                    {
                        UnEquipItem(equipSlots[(int)EquipSlot.ARMOR]);
                    }

                    equipSlots[(int)EquipSlot.ARMOR] = item;
                    itemDef += item.Def;
                }

                item.IsEquip = true;
            }

            public void UnEquipItem(Equip item)
            {
                if (item.Attack != -1)
                {
                    itemAttack -= item.Attack;
                    equipSlots[(int)EquipSlot.WEAPON] = null;
                }
                else
                {
                    itemDef -= item.Def;
                    equipSlots[(int)EquipSlot.ARMOR] = null;
                }

                item.IsEquip = false;
            }
        }

        public class Monster : ICharacter
        {
            private string? name;
            private int health;
            private float attack;
            private bool isDead;
            public string Name { get { return name ?? "Unknown"; } set { name = value; } }
            public int Health { get { return health; } set { health = value; } }
            public float Attack { get { return attack; } set { attack = value; } }
            public bool IsDead { get { return isDead; } set { isDead = value; } }

            public void TakeDamage(int damage)
            {
                health -= damage;
            }
        }

        public class Goblin : Monster
        {

        }

        public class Dragon : Monster
        {

        }
        #endregion

        #region Item
        public class HealthPotion : IItem
        {
            private string? name;
            public string Name { get { return name ?? "Unknown"; } set { name = value; } }

            public void Use(Warrior warrior)
            {
                warrior.Health += 20;
            }
        }

        public class StrengthPotion : IItem
        {
            private string? name;
            public string Name { get { return name ?? "Unknown"; } set { name = value; } }

            public void Use(Warrior warrior)
            {
                warrior.Attack += 2;
            }
        }

        public class Equip : IItem
        {
            ItemInfo tItemInfo;

            private int? attack;
            private int? def;
            private bool isEquip = false;

            public string Name { get { return tItemInfo.name ?? "Unknown"; } set { tItemInfo.name = value; } }
            public string Text { get { return tItemInfo.text ?? "Unknown"; } set { tItemInfo.text = value; } }
            public int Attack { get { return attack ?? -1; } set { attack = value; } }
            public int Def { get { return def ?? -1; } set { def = value; } }
            public int Price { get { return tItemInfo.price; } set { tItemInfo.price = value; } }
            public bool IsBuy { get { return tItemInfo.isBuy; } set { tItemInfo.isBuy = value; } }
            public bool IsEquip { get { return isEquip; } set { isEquip = value; } }

            void IItem.Use(Warrior warrior)
            {
                
            }
        }
        #endregion

        #region Stage
        public class Stage
        {
            private EventManager eventManager;
            private Warrior player;
            private Monster goblin;
            private Monster dragon;
            private HealthPotion healthPotion;
            private StrengthPotion strengthPotion;
            private Data data;
            private Shop shop;

            private int clearCount;

            public Stage(EventManager manager)
            {
                eventManager = manager;
                player = new Warrior() { Name = "전사", Level = 1, Attack = 10, Def = 5, Health = 100, Gold = 1500 };
                goblin = new Monster() { Name = "고블린", Attack = 10, Health = 50 };
                dragon = new Monster() { Name = "드래곤", Attack = 20, Health = 100 };
                healthPotion = new HealthPotion() { Name = "체력 포션" };
                strengthPotion = new StrengthPotion() { Name = "공격력 포션" };

                data = new();
                shop = new Shop(this, player, data);
                clearCount = 0;
            }

            public int Input()
            {
                Console.WriteLine("\n원하시는 행동을 입력해주세요.");
                Console.Write(">> ");

                string strInput = "";
                while (strInput == "")
                {
                    strInput = Console.ReadLine();
                }

                int input = int.Parse(strInput);

                return input;
            }

            public void MainScreen()
            {
                Console.Clear();

                Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
                Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");

                Console.WriteLine("1. 상태 보기");
                Console.WriteLine("2. 인벤토리");
                Console.WriteLine("3. 상점");
                Console.WriteLine("4. 던전입장");
                Console.WriteLine("5. 휴식");
                Console.WriteLine("6. 몬스터와 전투");
                Console.WriteLine("7. 저장하기");
                Console.WriteLine("8. 불러오기");
                Console.WriteLine("0. 게임종료");

                switch (Input())
                {
                    case 1:
                        ShowStatus();
                        break;
                    case 2:
                        OpenInven();
                        break;
                    case 3:
                        shop.OpenShop();
                        break;
                    case 4:
                        EnterDungeon();
                        break;
                    case 5:
                        Rest();
                        break;
                    case 6:
                        Start();
                        break;
                    case 7:
                        Save();
                        break;
                    case 8:
                        Load();
                        break;
                    case 0:
                        eventManager.IsStartGame = false;
                        break;
                    default:
                        Console.WriteLine("잘못된 입력입니다.");
                        Thread.Sleep(500);
                        break;
                }
            }
            
            public void ShowStatus()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("상태 보기");
                    Console.WriteLine("캐릭터의 정보가 표시됩니다.\n");

                    Console.WriteLine("Lv. {0}", player.Level);
                    Console.WriteLine("Chad ( {0} )", player.Name);

                    if (player.EquipSlots[(int)EquipSlot.WEAPON] == null)
                        Console.WriteLine("공격력 : {0}", player.Attack);
                    else
                        Console.WriteLine("공격력 : {0} (+{1})", player.Attack + player.EquipSlots[(int)EquipSlot.WEAPON].Attack, player.EquipSlots[(int)EquipSlot.WEAPON].Attack);

                    if (player.EquipSlots[(int)EquipSlot.ARMOR] == null)
                        Console.WriteLine("방어력 : {0}", player.Def);
                    else
                        Console.WriteLine("방어력 : {0} (+{1})", player.Def + player.EquipSlots[(int)EquipSlot.ARMOR].Def, player.EquipSlots[(int)EquipSlot.ARMOR].Def);

                    Console.WriteLine("체력 : {0}", player.Health);
                    Console.WriteLine("Gold : {0} G\n", player.Gold);

                    Console.WriteLine("0. 나가기");

                    switch (Input())
                    {
                        case 0:
                            isEscape = true;
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            Thread.Sleep(500);
                            break;
                    }
                }
            }

            public void OpenInven()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("인벤토리");
                    Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.\n");

                    Console.WriteLine("[아이템 목록]\n");

                    for (int i = 0; i < player.MyEquips.Count; ++i)
                    {
                        if (player.MyEquips[i].Attack != -1)
                        {
                            if (player.MyEquips[i].IsEquip)
                                Console.WriteLine("- [E]{0} |  공격력 +{1}  | {2} ", player.MyEquips[i].Name, player.MyEquips[i].Attack, player.MyEquips[i].Text);
                            else
                                Console.WriteLine("- {0} |  공격력 +{1}  | {2} ", player.MyEquips[i].Name, player.MyEquips[i].Attack, player.MyEquips[i].Text);
                        }
                        else if (player.MyEquips[i].Def != -1)
                        {
                            if (player.MyEquips[i].IsEquip)
                                Console.WriteLine("- [E]{0} |  방어력 +{1}  | {2} ", player.MyEquips[i].Name, player.MyEquips[i].Def, player.MyEquips[i].Text);
                            else
                                Console.WriteLine("- {0} |  방어력 +{1}  | {2} ", player.MyEquips[i].Name, player.MyEquips[i].Def, player.MyEquips[i].Text);
                        }
                    }

                    Console.WriteLine("\n1. 장착 관리");
                    Console.WriteLine("0. 나가기");

                    switch (Input())
                    {
                        case 1:
                            EquipManagement();
                            break;
                        case 0:
                            isEscape = true;
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            Thread.Sleep(500);
                            break;
                    }
                }
            }

            public void EquipManagement()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("인벤토리 - 장착 관리");
                    Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.\n");

                    Console.WriteLine("[아이템 목록]\n");

                    // TODO : 아이템 목록 출력
                    for (int i = 0; i < player.MyEquips.Count; ++i)
                    {
                        if (player.MyEquips[i].Attack != -1)
                        {
                            if (player.MyEquips[i].IsEquip)
                                Console.WriteLine("- {0} [E]{1} |  공격력 +{2}  | {3}", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Attack, player.MyEquips[i].Text);
                            else
                                Console.WriteLine("- {0} {1} |  공격력 +{2}  | {3}", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Attack, player.MyEquips[i].Text);
                        }
                        else if (player.MyEquips[i].Def != -1)
                        {
                            if (player.MyEquips[i].IsEquip)
                                Console.WriteLine("- {0} [E]{1} |  방어력 +{2}  | {3}", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Def, player.MyEquips[i].Text);
                            else
                                Console.WriteLine("- {0} {1} |  방어력 +{2}  | {3}", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Def, player.MyEquips[i].Text);
                        }

                    }

                    Console.WriteLine("\n0. 나가기");

                    int input = Input();

                    if (input <= player.MyEquips.Count)
                    {
                        if (input == 0)
                        {
                            isEscape = true;
                            break;
                        }
                        else
                        {
                            if (!player.MyEquips[input - 1].IsEquip)
                            {
                                player.EquipItem(player.MyEquips[input - 1]);
                            }
                            else
                            {
                                player.UnEquipItem(player.MyEquips[input - 1]);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("잘못된 입력입니다.");
                        Thread.Sleep(500);
                    }
                }
            }
            
            public void Rest()
            {
                bool isEscape = false;

                const int GOLD = 500;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("휴식하기");
                    Console.WriteLine("{0} G 를 내면 체력을 회복할 수 있습니다. ( 보유 골드 : {1} G )\n", GOLD, player.Gold);

                    Console.WriteLine("1. 휴식하기");
                    Console.WriteLine("0. 나가기");

                    switch (Input())
                    {
                        case 0:
                            isEscape = true;
                            break;
                        case 1:
                            {
                                if(player.Gold >= GOLD)
                                {
                                    player.TakeRest(GOLD);
                                    Console.WriteLine("휴식을 완료했습니다.");
                                }
                                else
                                {
                                    Console.WriteLine("Gold 가 부족합니다.");
                                }
                            }
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            break;
                    }

                    if (isEscape) break;

                    Thread.Sleep(500);
                }
            }

            public void EnterDungeon()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("던전입장");
                    Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");

                    Console.WriteLine("1. 쉬운 던전     | 방어력 5 이상 권장");
                    Console.WriteLine("2. 일반 던전     | 방어력 11 이상 권장");
                    Console.WriteLine("3. 어려운 던전   | 방어력 17 이상 권장");
                    Console.WriteLine("0. 나가기");

                    switch (Input())
                    {
                        case 0:
                            isEscape = true;
                            break;
                        case 1:
                            ResultScreen(Dungeon.EASY);
                            break;
                        case 2:
                            ResultScreen(Dungeon.NORMAL);
                            break;
                        case 3:
                            ResultScreen(Dungeon.HARD);
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            Thread.Sleep(500);
                            break;
                    }

                    if (isEscape) break;
                }
            }

            public void ResultScreen(Dungeon dungeon)
            {
                Console.Clear();

                bool isClear = true;
                Random rand = new Random();

                // 권장 방어력보다 낮을 때
                if (player.Def < (int)dungeon)
                {
                    int randResult = rand.Next(0, 100);

                    if (randResult < 40)
                        isClear = false;
                }

                if (isClear)
                {
                    int offset = player.Def;
                    int bonusGold = 0;
                    int bonusRatio = rand.Next((int)player.Attack, (int)player.Attack * 2 + 1);
                    float ratio = (100 + bonusRatio) / (float)100;

                    switch (dungeon)
                    {
                        case Dungeon.EASY:
                            offset -= (int)Dungeon.EASY;
                            bonusGold = (int)(1000 * ratio);
                            break;
                        case Dungeon.NORMAL:
                            offset -= (int)Dungeon.NORMAL;
                            bonusGold = (int)(1700 * ratio);
                            break;
                        case Dungeon.HARD:
                            offset -= (int)Dungeon.HARD;
                            bonusGold = (int)(2500 * ratio);
                            break;
                    }

                    int min = 20, max = 35;
                    int randHealth = rand.Next(min - offset, max + 1 - offset);

                    player.TakeDamage(randHealth);
                    player.Gold += bonusGold;
                    ++clearCount;

                    bool isLevelUp = false;
                    if (clearCount == player.Level)
                    {
                        player.LevelUp();
                        clearCount = 0;
                        isLevelUp = true;
                    }

                    bool isEscape = false;

                    while (!isEscape)
                    {
                        Console.Clear();

                        Console.WriteLine("던전 클리어");
                        Console.WriteLine("축하합니다!!");

                        switch (dungeon)
                        {
                            case Dungeon.EASY:
                                Console.WriteLine("쉬운 던전을 클리어 하였습니다.\n");
                                break;
                            case Dungeon.NORMAL:
                                Console.WriteLine("일반 던전을 클리어 하였습니다.\n");
                                break;
                            case Dungeon.HARD:
                                Console.WriteLine("어려운 던전을 클리어 하였습니다.\n");
                                break;
                        }

                        Console.WriteLine("[탐험 결과]");
                        Console.WriteLine("체력 {0} -> {1}", player.Health + randHealth, player.Health);
                        Console.WriteLine("Gold {0} G -> {1} G", player.Gold - bonusGold, player.Gold);

                        if (isLevelUp)
                            Console.WriteLine("\n ★★★ 레벨이 올랐습니다!!! ★★★ ( Lv. {0} -> {1} )\n", player.Level - 1, player.Level);
                        
                        Console.WriteLine("\n0. 나가기");

                        switch (Input())
                        {
                            case 0:
                                isEscape = true;
                                break;
                            default:
                                Console.WriteLine("잘못된 입력입니다.");
                                break;
                        }

                        if (isEscape) break;

                        Thread.Sleep(500);
                    }
                }
                else
                {
                    // 던전 실패
                    bool isEscape = false;

                    player.Health = (int)(player.Health * 0.5f);

                    while (!isEscape)
                    {
                        Console.Clear();

                        Console.WriteLine("던전 실패...");
                        Console.WriteLine("공략에 실패 했습니다...\n");

                        Console.WriteLine("[탐험 결과]");
                        Console.WriteLine("체력 {0} -> {1}\n", player.Health * 2, player.Health);

                        Console.WriteLine("0. 나가기");

                        switch (Input())
                        {
                            case 0:
                                isEscape = true;
                                break;
                            default:
                                Console.WriteLine("잘못된 입력입니다.");
                                break;
                        }

                        if (isEscape) break;

                        Thread.Sleep(500);
                    }
                }
            }

            public void Start()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("몬스터와 전투");
                    Console.WriteLine("몬스터와 교대로 전투를 진행합니다.\n");

                    Console.WriteLine("[몬스터 목록]");
                    Console.WriteLine("1. 고블린");
                    Console.WriteLine("2. 드래곤\n");

                    Console.WriteLine("0. 나가기");

                    switch (Input())
                    {
                        case 0:
                            isEscape = true;
                            break;
                        case 1:
                            Battle(goblin);
                            break;
                        case 2:
                            Battle(dragon);
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            Thread.Sleep(500);
                            break;
                    }
                }
            }

            public void Battle(Monster monster)
            {
                bool isEscape = false;

                int maxHp = monster.Health;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("몬스터와 전투");
                    Console.WriteLine("몬스터와 교대로 전투를 진행합니다.\n");

                    if (monster.Name == "고블린")
                    {
                        Console.WriteLine("[고블린]");
                        Console.WriteLine("남은 체력 : {0}", monster.Health);
                    }
                    else if (monster.Name == "드래곤")
                    {
                        Console.WriteLine("[드래곤]");
                        Console.WriteLine("남은 체력 : {0}", monster.Health);
                    }

                    Console.WriteLine("\n플레이어 체력 : {0}", player.Health);

                    Console.WriteLine("\n1. 전투");
                    Console.WriteLine("0. 나가기");

                    switch (Input())
                    {
                        case 0:
                            monster.Health = maxHp;
                            isEscape = true;
                            break;
                        case 1:
                            {
                                Console.WriteLine("\n플레이어 공격! 몬스터에게 {0} 의 데미지", player.Attack);
                                Thread.Sleep(500);
                                Console.WriteLine("\n몬스터 공격! 플레이어에게 {0} 의 데미지", monster.Attack);
                                Thread.Sleep(500);

                                player.TakeDamage((int)monster.Attack);
                                monster.TakeDamage((int)player.Attack);

                                if (player.Health <= 0)
                                {
                                    player.IsDead = true;
                                }
                                else if (monster.Health <= 0)
                                {
                                    monster.IsDead = true;
                                }

                                if(player.IsDead)
                                {
                                    Console.WriteLine("\n플레이어 사망... 던전 입구로 돌아갑니다.");
                                    player.Health = 10;
                                    player.IsDead = false;
                                    Thread.Sleep(1000);

                                    isEscape = true;
                                }
                                else if(monster.IsDead)
                                {
                                    monster.Health = maxHp;
                                    monster.IsDead = false;

                                    GetReward();

                                    isEscape = true;
                                }
                            }
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            Thread.Sleep(500);
                            break;
                    }
                }
            }

            public void GetReward()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("몬스터와 전투");
                    Console.WriteLine("몬스터와 교대로 전투를 진행합니다.\n");

                    Console.WriteLine("\n몬스터 토벌 완료!");
                    Console.WriteLine("보상을 선택하세요.");

                    Console.WriteLine("\n1. 체력 포션");
                    Console.WriteLine("2. 공격력 포션");
                    Console.WriteLine("0. 나가기");

                    switch (Input())
                    {
                        case 0:
                            isEscape = true;
                            break;
                        case 1:
                            healthPotion.Use(player);
                            Console.WriteLine("\n체력 포션을 획득");
                            Thread.Sleep(1000);
                            isEscape = true;
                            break;
                        case 2:
                            strengthPotion.Use(player);
                            Console.WriteLine("\n공격력 포션을 획득");
                            Thread.Sleep(1000);
                            isEscape = true;
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            Thread.Sleep(500);
                            break;
                    }
                }
            }

            public void Save()
            {
                shop.Save();

                data.name       = player.Name;
                data.level      = player.Level;
                data.health     = player.Health;
                data.attack     = player.Attack;
                data.itemAttack = player.ItemAttack;
                data.def        = player.Def;
                data.itemDef    = player.ItemDef;
                data.gold       = player.Gold;

                for(int i = 0; i < player.MyEquips.Count; i++)
                    data.myEquips.Add(player.MyEquips[i]);

                data.equipSlots[(int)EquipSlot.WEAPON] = player.EquipSlots[(int)EquipSlot.WEAPON];
                data.equipSlots[(int)EquipSlot.ARMOR] = player.EquipSlots[(int)EquipSlot.ARMOR];

                SaveData(data);

                Console.WriteLine("데이터 저장 성공!");
                Thread.Sleep(500);
            }

            public void Load()
            {
                data = LoadData();
                shop.Load(data);

                player.Name = data.name;
                player.Level = data.level;
                player.Health = data.health;
                player.Attack = data.attack;
                player.ItemAttack = data.itemAttack;
                player.Def = data.def;
                player.ItemDef = data.itemDef;
                player.Gold = data.gold;

                player.MyEquips.Clear();

                for (int i = 0; i < data.myEquips.Count; i++)
                    player.MyEquips.Add(data.myEquips[i]);

                player.EquipSlots[(int)EquipSlot.WEAPON] = data.equipSlots[(int)EquipSlot.WEAPON];
                player.EquipSlots[(int)EquipSlot.ARMOR] = data.equipSlots[(int)EquipSlot.ARMOR];

                Console.WriteLine("데이터 로드 성공!");
                Thread.Sleep(500);
            }
        }
        #endregion

        #region Shop
        public class Shop
        {
            private Stage mainStage;
            private Warrior player;
            private Data data;
            private List<Equip> equips = new List<Equip>();

            public Shop(Stage stage, Warrior warrior, Data data)
            {
                mainStage = stage;
                player = warrior;
                this.data = data;

                equips.Add(new Equip() { Name = "수련자 갑옷    ", Def = 5, Text = "수련에 도움을 주는 갑옷입니다.                    ", Price = 1000 });
                equips.Add(new Equip() { Name = "무쇠 갑옷      ", Def = 9, Text = "무쇠로 만들어져 튼튼한 갑옷입니다.                ", Price = 2000 });
                equips.Add(new Equip() { Name = "스파르타의 갑옷", Def = 15, Text = "스파르타의 전사들이 사용했다는 전설의 갑옷입니다.", Price = 3500 });
                equips.Add(new Equip() { Name = "낡은 검        ", Attack = 2, Text = "쉽게 볼 수 있는 낡은 검 입니다.                   ", Price = 600 });
                equips.Add(new Equip() { Name = "청동 도끼      ", Attack = 5, Text = "어디선가 사용됐던거 같은 도끼입니다.              ", Price = 1500 });
                equips.Add(new Equip() { Name = "스파르타의 창  ", Attack = 7, Text = "스파르타의 전사들이 사용했다는 전설의 창입니다.   ", Price = 3000 });
            }

            public void OpenShop()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("상점");
                    Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.\n");

                    Console.WriteLine("[보유 골드]");
                    Console.WriteLine(" {0} G", player.Gold);

                    Console.WriteLine("\n[아이템 목록]");

                    for(int i = 0; i < equips.Count; ++i)
                    {
                        if(!equips[i].IsBuy)
                        {
                            if (equips[i].Attack != -1)
                                Console.WriteLine("- {0} |  공격력 +{1}  | {2} | {3} G", equips[i].Name, equips[i].Attack, equips[i].Text, equips[i].Price);
                            else if (equips[i].Def != -1)
                                Console.WriteLine("- {0} |  방어력 +{1}  | {2} | {3} G", equips[i].Name, equips[i].Def, equips[i].Text, equips[i].Price);
                        }
                        else
                        {
                            if (equips[i].Attack != -1)
                                Console.WriteLine("- {0} |  공격력 +{1}  | {2} | 구매완료", equips[i].Name, equips[i].Attack, equips[i].Text);
                            else if (equips[i].Def != -1)              
                                Console.WriteLine("- {0} |  방어력 +{1}  | {2} | 구매완료", equips[i].Name, equips[i].Def, equips[i].Text);
                        }
                        
                    }

                    Console.WriteLine("\n1. 아이템 구매");
                    Console.WriteLine("2. 아이템 판매");
                    Console.WriteLine("0. 나가기");

                    switch (mainStage.Input())
                    {
                        case 1:
                            BuyItemScreen();
                            break;
                        case 2:
                            SellItemScreen();
                            break;
                        case 0:
                            isEscape = true;
                            break;
                        default:
                            Console.WriteLine("잘못된 입력입니다.");
                            Thread.Sleep(500);
                            break;
                    }
                }
            }

            public void BuyItemScreen()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("상점 - 아이템 구매");
                    Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.\n");

                    Console.WriteLine("[보유 골드]");
                    Console.WriteLine(" {0} G", player.Gold);

                    Console.WriteLine("\n[아이템 목록]");

                    for (int i = 0; i < equips.Count; ++i)
                    {
                        if (!equips[i].IsBuy)
                        {
                            if (equips[i].Attack != -1)
                                Console.WriteLine("- {0} {1} |  공격력 +{2}  | {3} | {4} G", i + 1, equips[i].Name, equips[i].Attack, equips[i].Text, equips[i].Price);
                            else if (equips[i].Def != -1)
                                Console.WriteLine("- {0} {1} |  방어력 +{2}  | {3} | {4} G", i + 1, equips[i].Name, equips[i].Def, equips[i].Text, equips[i].Price);
                        }
                        else
                        {
                            if (equips[i].Attack != -1)
                                Console.WriteLine("- {0} {1} |  공격력 +{2}  | {3} | 구매완료", i + 1, equips[i].Name, equips[i].Attack, equips[i].Text);
                            else if (equips[i].Def != -1)
                                Console.WriteLine("- {0} {1} |  방어력 +{2}  | {3} | 구매완료", i + 1, equips[i].Name, equips[i].Def, equips[i].Text);
                        }

                    }

                    Console.WriteLine("\n0. 나가기");

                    int input = mainStage.Input();

                    if (input <= equips.Count)
                    {
                        if (input == 0)
                        {
                            isEscape = true;
                            break;
                        }
                        else
                        {
                            if(!equips[input - 1].IsBuy)
                            {
                                if(player.Gold >= equips[input - 1].Price)
                                {
                                    player.BuyItem(equips[input - 1]);
                                    
                                    Console.WriteLine("구매를 완료했습니다.");
                                }
                                else
                                {
                                    Console.WriteLine("Gold가 부족합니다.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("이미 구매한 아이템입니다.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("잘못된 입력입니다.");
                    }

                    Thread.Sleep(500);
                }
            }
            public void SellItemScreen()
            {
                bool isEscape = false;

                while (!isEscape)
                {
                    Console.Clear();

                    Console.WriteLine("상점 - 아이템 판매");
                    Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.\n");

                    Console.WriteLine("[보유 골드]");
                    Console.WriteLine(" {0} G", player.Gold);

                    Console.WriteLine("\n[아이템 목록]");

                    for (int i = 0; i < player.MyEquips.Count; ++i)
                    {
                        if (player.MyEquips[i].Attack != -1)
                        {
                            if (player.MyEquips[i].IsEquip)
                                Console.WriteLine("- {0} [E]{1} |  공격력 +{2}  | {3} | {4} G", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Attack, player.MyEquips[i].Text, player.MyEquips[i].Price * 0.85f);
                            else
                                Console.WriteLine("- {0} {1} |  공격력 +{2}  | {3} | {4} G", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Attack, player.MyEquips[i].Text, player.MyEquips[i].Price * 0.85f);
                        }
                        else if (player.MyEquips[i].Def != -1)
                        {
                            if (player.MyEquips[i].IsEquip)
                                Console.WriteLine("- {0} [E]{1} |  방어력 +{2}  | {3} | {4} G", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Def, player.MyEquips[i].Text, player.MyEquips[i].Price * 0.85f);
                            else
                                Console.WriteLine("- {0} {1} |  방어력 +{2}  | {3} | {4} G", i + 1, player.MyEquips[i].Name, player.MyEquips[i].Def, player.MyEquips[i].Text, player.MyEquips[i].Price * 0.85f);
                        }
                    }

                    Console.WriteLine("\n0. 나가기");

                    int input = mainStage.Input();

                    if (input <= player.MyEquips.Count)
                    {
                        if (input == 0)
                        {
                            isEscape = true;
                            break;
                        }
                        else
                        {
                            if (player.MyEquips[input-1].IsEquip)
                            {
                                player.UnEquipItem(player.MyEquips[input - 1]);
                            }

                            player.SellItem(player.MyEquips[input - 1]);

                            Console.WriteLine("판매 완료.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("잘못된 입력입니다.");
                    }

                    Thread.Sleep(500);
                }
            }

            public void Save()
            {
                for(int i = 0; i < equips.Count; ++i)
                    data.shopEquips.Add(equips[i]);
            }

            public void Load(Data loadData)
            {
                data = loadData;

                equips.Clear();

                for (int i = 0; i < data.shopEquips.Count; ++i)
                    equips.Add(data.shopEquips[i]);
            }
        }
        #endregion

        #region EventManager

        public delegate void GameEvent();
        public class EventManager
        {
            public event GameEvent? OnGameStart;
            public event GameEvent? OnGameEnd;

            private Stage stage;

            private bool isStartGame = false;

            public Stage Stage { get { return stage; } set { stage = value; } }
            public bool IsStartGame { get { return isStartGame; } set { isStartGame = value; } }

            public EventManager()
            {
                stage = new Stage(this);
            }

            public void RunGame()
            {
                OnGameStart?.Invoke();

                while (isStartGame)
                {
                    stage.MainScreen();
                }

                OnGameEnd?.Invoke();
            }
        }
        #endregion

        #region Save & Load

        public class Data
        {
            public string? name;
            public int level;
            public int health;
            public float attack;
            public int itemAttack;
            public int def;
            public int itemDef;
            public int gold;

            public List<Equip> shopEquips = new List<Equip>();
            public List<Equip> myEquips = new List<Equip>();
            public Equip[] equipSlots = new Equip[(int)EquipSlot.NONE];
        }

        public static string path = Environment.CurrentDirectory;

        public static string SettingPath(string path)
        {
            string tempPath = path;
            string dir = "json";

            for (int i = 0; i < 3; ++i)
            {
                tempPath = Path.GetDirectoryName(tempPath);
            }

            return Path.Combine(tempPath, dir);
        }

        public static void SaveData(Data data)
        {
            string saveData = JsonConvert.SerializeObject(data);
            File.WriteAllText(SettingPath(path) + "\\SaveData.json", saveData);
        }

        public static Data LoadData()
        {
            string tempPath = SettingPath(path) + "\\SaveData.json";

            try
            {
                if (File.Exists(tempPath))
                {
                    string strLoadData = File.ReadAllText(tempPath);
                    Data loadData = JsonConvert.DeserializeObject<Data>(strLoadData);

                    if (loadData == null)
                        throw new LoadFailException("로드에 실패했습니다...");

                    return loadData;
                }
                else
                {
                    throw new LoadFailException("로드에 실패했습니다...");
                }
            }
            catch (LoadFailException ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
            catch(Exception ex)
            {
                Console.WriteLine("예외가 발생했습니다 : " + ex.Message);

                return null;
            }
        }
        #endregion

        #region Exception

        public class LoadFailException : Exception
        { 
            public LoadFailException(string message) : base(message) 
            { 
            } 
        }

        #endregion

        static void Main(string[] args)
        {
            EventManager eventManager = new EventManager();
            Data data = new();

            eventManager.OnGameStart += () => 
            { 
                eventManager.IsStartGame = true;
            };

            eventManager.OnGameEnd += () => 
            { 
                Console.WriteLine("게임 종료...");
                Thread.Sleep(500);
            };

            eventManager.RunGame();
        }
    }
}

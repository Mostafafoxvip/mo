#!/usr/bin/env python3
"""Lucky Quest – One-command mode: python3 play.py [spin|+|-|max|auto|stats]"""

import random, time, sys, os, json
from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich.text import Text
from rich.columns import Columns
from rich.align import Align
from rich import box

console = Console()
SAVE = "/tmp/lq_save.json"

SYMBOLS = [
    {"id":"man",     "emoji":"🤵", "weight":3,  "mult":15},
    {"id":"gem",     "emoji":"💎", "weight":6,  "mult":8},
    {"id":"wild",    "emoji":"⭐", "weight":5,  "mult":0,  "wild":True},
    {"id":"scatter", "emoji":"💫", "weight":4,  "mult":0,  "scatter":True},
    {"id":"chest",   "emoji":"📦", "weight":10, "mult":5},
    {"id":"hat",     "emoji":"🎩", "weight":12, "mult":4},
    {"id":"coin",    "emoji":"🪙", "weight":22, "mult":2},
    {"id":"ace",     "emoji":"🅰️", "weight":22, "mult":1.8},
    {"id":"king",    "emoji":"🎴", "weight":20, "mult":1.4},
    {"id":"nine",    "emoji":"9️⃣", "weight":18, "mult":1},
]
TOTAL_W = sum(s["weight"] for s in SYMBOLS)
BETS = [1_000,2_000,5_000,10_000,20_000,50_000,100_000,200_000]
JP = {"grand":5_093_630,"major":1_024_752,"minor":417_877,"mini":201_313}

def pick():
    r,cum=random.random()*TOTAL_W,0
    for s in SYMBOLS:
        cum+=s["weight"]
        if r<cum: return s
    return SYMBOLS[-1]

def fmt(n): return f"{int(n):,}"

def load():
    try:
        d=json.load(open(SAVE))
        return d
    except:
        return {"balance":1_960_000,"bet_idx":4,"spins":0,
                "total_won":0,"best_win":0,"streak":0,"free_spins":0,
                "grid":[[pick()["id"] for _ in range(3)] for _ in range(5)]}

def save(st):
    json.dump(st,open(SAVE,"w"))

def id2sym(sid):
    for s in SYMBOLS:
        if s["id"]==sid: return s
    return SYMBOLS[-1]

def grid_from_ids(ids):
    return [[id2sym(ids[c][r]) for r in range(3)] for c in range(5)]

def grid_to_ids(grid):
    return [[grid[c][r]["id"] for r in range(3)] for c in range(5)]

def evaluate(grid,bet):
    best_win,best_hl=0,set()
    for row in range(3):
        line=[grid[c][row] for c in range(5)]
        base,count=None,0
        for s in line:
            if s.get("wild"):count+=1;continue
            if s.get("scatter"):continue
            if base is None:base=s;count+=1
            elif s["id"]==base["id"]:count+=1
            else:break
        if base and count>=3:
            win=int(bet*base["mult"]*count)
            if win>best_win:
                best_win=win
                best_hl={(c,row) for c in range(count)}
    return best_win,best_hl

def count_scatters(grid):
    return sum(1 for c in grid for s in c if s.get("scatter"))

def check_jackpot(grid):
    all_s=[s for c in grid for s in c]
    first=next((s for s in all_s if not s.get("wild") and not s.get("scatter")),None)
    if first and all(s["id"]==first["id"] or s.get("wild") for s in all_s):
        if first["id"]=="man":return "grand"
        if first["id"]=="gem":return "major"
    return None

def draw(grid, st, msg="", hl=None):
    console.print()
    console.print(Align.center(Text("🎰  LUCKY QUEST  🎰",style="bold yellow on dark_magenta")))
    console.print()

    jp=JP
    console.print(Columns([
        Panel(f"[bold white]{fmt(jp['mini'])}[/]",  title="[green]MINI[/]",  border_style="green",  expand=True),
        Panel(f"[bold white]{fmt(jp['minor'])}[/]", title="[cyan]MINOR[/]",  border_style="cyan",   expand=True),
        Panel(f"[bold white]{fmt(jp['major'])}[/]", title="[yellow]MAJOR[/]",border_style="yellow", expand=True),
        Panel(f"[bold white]{fmt(jp['grand'])}[/]", title="[red]GRAND[/]",   border_style="red",    expand=True),
    ],equal=True))
    console.print()

    t=Table(show_header=False,box=box.HEAVY_HEAD,border_style="gold1",
            expand=False,padding=(0,2))
    for _ in range(5):t.add_column(justify="center",width=9)

    for row in range(3):
        cells=[]
        for col in range(5):
            sym=grid[col][row]
            h=hl and (col,row) in hl
            bg="on yellow" if h else("on purple4" if sym.get("scatter") else "on grey15")
            cells.append(Text(f" {sym['emoji']} ",style=f"bold {bg}",justify="center"))
        t.add_row(*cells)
    console.print(Align.center(t))
    console.print()

    streak=st["streak"]
    dots="".join("🟡" if i<streak else "⚫" for i in range(5))
    mult=[1,1,1.5,2,3,4][min(streak,5)]
    console.print(Align.center(Text(f"🔥 STREAK  {dots}  ×{mult}",style="bold yellow")))
    console.print()

    fs=""
    if st["free_spins"]>0:
        fs=f"  🌟 [bold cyan]FREE SPINS: {st['free_spins']}[/]"
    bc="green" if st["balance"]>500_000 else "red"
    console.print(Align.center(
        f"💰 [{bc}]{fmt(st['balance'])}[/{bc}]  🎯 Bet:[bold]{fmt(BETS[st['bet_idx']])}[/]"
        f"  📊 Spins:[bold]{st['spins']}[/]  🏆 Best:[bold yellow]{fmt(st['best_win'])}[/]{fs}"
    ))
    console.print()

    if msg:
        console.print(Align.center(Panel(msg,border_style="gold1",expand=False)))
        console.print()

    console.print(Align.center(
        "[bold cyan]اكتب الأمر:[/]  [bold]spin[/] أو [bold]s[/] (دوران)  "
        "[bold]+[/] رفع رهان  [bold]-[/] تخفيض  [bold]max[/] أقصى  "
        "[bold]auto[/] ×10  [bold]buy[/] شراء عملات"
    ))
    console.print()

def do_spin(st):
    grid=grid_from_ids(st["grid"])
    is_free=st["free_spins"]>0
    bet=BETS[st["bet_idx"]]
    msg_parts=[]

    if not is_free:
        if st["balance"]<bet:
            return grid,"❌ رصيدك غير كافٍ! اشتري عملات بـ [bold]buy[/]"
        st["balance"]-=bet
    else:
        st["free_spins"]-=1
        msg_parts.append(f"✨ Free Spin! متبقي: {st['free_spins']}")

    st["spins"]+=1

    final=[[pick() for _ in range(3)] for _ in range(5)]
    st["grid"]=grid_to_ids(final)

    sc=count_scatters(final)
    if sc>=3:
        fn={3:10,4:15}.get(sc,20)
        st["free_spins"]+=fn
        return final,f"💫 [bold magenta]SCATTER × {sc}  →  {fn} FREE SPINS UNLOCKED![/] 🎉"

    jp_type=check_jackpot(final)
    if jp_type:
        amount=JP[jp_type]
        st["balance"]+=amount
        st["total_won"]+=amount
        if amount>st["best_win"]:st["best_win"]=amount
        st["streak"]=min(5,st["streak"]+1)
        c={"grand":"red","major":"yellow","minor":"cyan","mini":"green"}[jp_type]
        return final,f"👑 [bold {c}]{jp_type.upper()} JACKPOT!  +{fmt(amount)}[/] 🎊🎊🎊"

    mult=[1,1,1.5,2,3,4][min(st["streak"],5)]
    win,hl=evaluate(final,bet)
    if win>0:
        fw=int(win*mult)
        st["balance"]+=fw
        st["total_won"]+=fw
        if fw>st["best_win"]:st["best_win"]=fw
        st["streak"]=min(5,st["streak"]+1)
        lbl="🎉 MEGA WIN!" if fw>=bet*40 else("🔥 BIG WIN!" if fw>=bet*15 else "✅ WIN!")
        ms=f"{lbl}  [bold yellow]+{fmt(fw)}[/]"
        if mult>1:ms+=f"  [bold cyan](streak ×{mult})[/]"
        if msg_parts:ms=msg_parts[0]+"  "+ms
        return final,ms
    else:
        st["streak"]=0
        m="😔 لا يوجد فوز"
        if msg_parts:m=msg_parts[0]+"  "+m
        return final,m

def main():
    st=load()
    cmd=(sys.argv[1].lower().strip() if len(sys.argv)>1 else "show")
    grid=grid_from_ids(st["grid"])
    msg=""
    hl=None

    if cmd in("spin","s","دور","دوران"):
        grid,msg=do_spin(st)
    elif cmd=="+":
        st["bet_idx"]=min(len(BETS)-1,st["bet_idx"]+1)
        msg=f"✅ الرهان: [bold]{fmt(BETS[st['bet_idx']])}[/]"
    elif cmd=="-":
        st["bet_idx"]=max(0,st["bet_idx"]-1)
        msg=f"✅ الرهان: [bold]{fmt(BETS[st['bet_idx']])}[/]"
    elif cmd=="max":
        st["bet_idx"]=len(BETS)-1
        msg=f"💥 MAX BET: [bold]{fmt(BETS[st['bet_idx']])}[/]"
    elif cmd=="buy":
        st["balance"]+=500_000
        msg="💰 [bold green]+500,000 عملة أضيفت![/]"
    elif cmd=="auto":
        msgs=[]
        for _ in range(10):
            grid,m=do_spin(st)
            msgs.append(m)
        msg="🤖 AUTO ×10\n"+"\n".join(f"  {m}" for m in msgs[-3:])
    elif cmd=="stats":
        msg=(f"📊 الإجمالي: [yellow]{fmt(st['total_won'])}[/]  "
             f"أكبر فوز: [yellow]{fmt(st['best_win'])}[/]  "
             f"دورات: [yellow]{st['spins']}[/]")
    elif cmd=="reset":
        os.remove(SAVE) if os.path.exists(SAVE) else None
        st=load()
        msg="🔄 تم إعادة الضبط!"

    save(st)
    draw(grid,st,msg,hl)

if __name__=="__main__":
    main()

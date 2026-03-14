#!/usr/bin/env python3
"""Lucky Quest – Terminal Slot Machine"""

import random, time, sys, os
from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich.text import Text
from rich.columns import Columns
from rich import box
from rich.live import Live
from rich.align import Align
from rich.layout import Layout
from rich.style import Style
import threading

console = Console()

# ─── SYMBOLS ───────────────────────────────────────────
SYMBOLS = [
    {"id":"man",     "emoji":"🤵", "label":"MAN",     "weight":3,  "mult":15},
    {"id":"gem",     "emoji":"💎", "label":"GEM",     "weight":6,  "mult":8},
    {"id":"wild",    "emoji":"⭐", "label":"WILD",    "weight":5,  "mult":0,  "wild":True},
    {"id":"scatter", "emoji":"💫", "label":"SCATTER", "weight":4,  "mult":0,  "scatter":True},
    {"id":"chest",   "emoji":"📦", "label":"CHEST",   "weight":10, "mult":5},
    {"id":"hat",     "emoji":"🎩", "label":"HAT",     "weight":12, "mult":4},
    {"id":"coin",    "emoji":"🪙", "label":"COIN",    "weight":22, "mult":2},
    {"id":"ace",     "emoji":"🅰️", "label":"A",       "weight":22, "mult":1.8},
    {"id":"king",    "emoji":"🎴", "label":"K",       "weight":20, "mult":1.4},
    {"id":"nine",    "emoji":"9️⃣", "label":"9",       "weight":18, "mult":1},
]
TOTAL_W = sum(s["weight"] for s in SYMBOLS)
BETS    = [1_000, 2_000, 5_000, 10_000, 20_000, 50_000, 100_000]

# ─── STATE ─────────────────────────────────────────────
state = {
    "balance":  1_960_000,
    "bet_idx":  4,
    "spins":    0,
    "total_won":0,
    "best_win": 0,
    "streak":   0,
    "free_spins":0,
    "level":    1,
}

jp = {"grand":5_093_630, "major":1_024_752, "minor":417_877, "mini":201_313}

def pick():
    r, cum = random.random()*TOTAL_W, 0
    for s in SYMBOLS:
        cum += s["weight"]
        if r < cum: return s
    return SYMBOLS[-1]

def fmt(n): return f"{int(n):,}"

# ─── DRAW REELS ────────────────────────────────────────
def draw_reels(grid, highlights=None, msg=""):
    os.system("clear")

    # header
    console.print()
    console.print(Align.center(
        Text("🎰  LUCKY QUEST  🎰", style="bold yellow on dark_magenta")
    ))
    console.print()

    # jackpots
    jp_row = Columns([
        Panel(f"[bold white]{fmt(jp['mini'])}[/]",  title="[green]MINI[/]",  border_style="green",  expand=True),
        Panel(f"[bold white]{fmt(jp['minor'])}[/]", title="[cyan]MINOR[/]",  border_style="cyan",   expand=True),
        Panel(f"[bold white]{fmt(jp['major'])}[/]", title="[yellow]MAJOR[/]",border_style="yellow", expand=True),
        Panel(f"[bold white]{fmt(jp['grand'])}[/]", title="[red]GRAND[/]",   border_style="red",    expand=True),
    ], equal=True)
    console.print(jp_row)
    console.print()

    # reels table
    table = Table(show_header=False, box=box.HEAVY_HEAD,
                  border_style="gold1", expand=False, padding=(0,2))
    for _ in range(5): table.add_column(justify="center", width=9)

    for row in range(3):
        cells = []
        for col in range(5):
            sym = grid[col][row]
            hl  = highlights and (col, row) in highlights
            bg  = "on yellow" if hl else ("on purple4" if sym.get("scatter") else "on grey15")
            style = f"bold {bg}"
            cells.append(Text(f" {sym['emoji']} ", style=style, justify="center"))
        table.add_row(*cells)

    console.print(Align.center(table))
    console.print()

    # streak
    streak = state["streak"]
    dots = "".join("🟡" if i < streak else "⚫" for i in range(5))
    mult = [1,1,1.5,2,3,4][min(streak,5)]
    console.print(Align.center(
        Text(f"🔥 STREAK  {dots}  ×{mult}", style="bold yellow")
    ))
    console.print()

    # info bar
    bal_color = "green" if state["balance"] > 0 else "red"
    fs_info = f"  🌟 FREE SPINS: [bold cyan]{state['free_spins']}[/]" if state["free_spins"] > 0 else ""
    console.print(Align.center(
        f"💰 Balance: [{bal_color}]{fmt(state['balance'])}[/{bal_color}]   "
        f"🎯 Bet: [bold]{fmt(BETS[state['bet_idx']])}[/]   "
        f"📊 Spins: [bold]{state['spins']}[/]   "
        f"🏆 Best: [bold yellow]{fmt(state['best_win'])}[/]"
        f"{fs_info}"
    ))
    console.print()

    # message
    if msg:
        console.print(Align.center(Text(msg, style="bold white on dark_green")))
        console.print()

    # controls
    console.print(Align.center(
        "[dim]  [S] SPIN   [+/-] BET   [A] AUTO×10   [Q] QUIT  [/dim]"
    ))
    console.print()

# ─── SPIN ANIMATION ────────────────────────────────────
def animate_spin(final_grid):
    spin_syms = [s for s in SYMBOLS if not s.get("scatter")]
    frames = 12
    for f in range(frames):
        temp = [[pick() for _ in range(3)] for _ in range(5)]
        draw_reels(temp, msg="🎰  spinning...")
        # gradually lock columns
        if f >= 4:
            for col in range(min(f - 3, 5)):
                temp[col] = final_grid[col]
        time.sleep(0.07 + f*0.012)
    draw_reels(final_grid)

# ─── EVALUATE ──────────────────────────────────────────
def evaluate(grid, bet):
    best_win, best_hl = 0, set()
    for row in range(3):
        line = [grid[col][row] for col in range(5)]
        base, count = None, 0
        for s in line:
            if s.get("wild"): count += 1; continue
            if s.get("scatter"): continue
            if base is None: base = s; count += 1
            elif s["id"] == base["id"]: count += 1
            else: break
        if base and count >= 3:
            win = int(bet * base["mult"] * count)
            if win > best_win:
                best_win = win
                best_hl  = {(col, row) for col in range(count)}
    return best_win, best_hl

def count_scatters(grid):
    return sum(1 for col in grid for s in col if s.get("scatter"))

def check_jackpot(grid):
    all_syms = [s for col in grid for s in col]
    first = next((s for s in all_syms if not s.get("wild") and not s.get("scatter")), None)
    if first and all(s["id"]==first["id"] or s.get("wild") for s in all_syms):
        if first["id"] == "man":  return "grand"
        if first["id"] == "gem":  return "major"
    return None

# ─── WIN DISPLAY ───────────────────────────────────────
def show_win(win, label="WIN"):
    styles = {
        "MEGA WIN": "bold white on red",
        "BIG WIN":  "bold white on dark_orange",
        "WIN":      "bold black on yellow",
    }
    style = styles.get(label, "bold yellow")
    msg = f"  🏆  {label}  +{fmt(win)}  🏆  "
    for _ in range(3):
        console.print(Align.center(Text(msg, style=style)))
        time.sleep(0.18)
        console.print(Align.center(Text(" " * len(msg), style="on black")))
        time.sleep(0.1)
    console.print(Align.center(Text(msg, style=style)))

def show_jackpot(jp_type):
    colors = {"grand":"red","major":"yellow","minor":"cyan","mini":"green"}
    col = colors[jp_type]
    console.print()
    for _ in range(4):
        console.print(Align.center(
            Text(f"  👑  {jp_type.upper()} JACKPOT!  {fmt(jp[jp_type])}  👑  ",
                 style=f"bold white on {col}")
        ))
        time.sleep(0.25)
        console.print(Align.center(Text("", style="")))
        time.sleep(0.1)
    console.print(Align.center(
        Text(f"  👑  {jp_type.upper()} JACKPOT!  +{fmt(jp[jp_type])}  👑  ",
             style=f"bold white on {col}")
    ))
    time.sleep(2)

def show_free_spins(n):
    console.print()
    for _ in range(3):
        console.print(Align.center(
            Text(f"  ✨  FREE SPINS UNLOCKED!  {n} SPINS  ✨  ",
                 style="bold white on purple")
        ))
        time.sleep(0.3)
        console.print(Align.center(Text("", style="")))
        time.sleep(0.15)
    time.sleep(1)

# ─── SPIN ──────────────────────────────────────────────
def do_spin():
    is_free = state["free_spins"] > 0
    bet = BETS[state["bet_idx"]]

    if not is_free:
        if state["balance"] < bet:
            draw_reels(grid, msg="❌  رصيدك غير كافٍ! اضغط Q للخروج.")
            return False
        state["balance"] -= bet
    else:
        state["free_spins"] -= 1

    state["spins"] += 1

    # slight scatter boost
    boost = random.random() < 0.07
    final = [[pick() for _ in range(3)] for _ in range(5)]

    animate_spin(final)
    for col in range(5): grid[col] = final[col]

    # scatter check
    sc = count_scatters(final)
    if sc >= 3:
        free_n = {3:10, 4:15}.get(sc, 20)
        state["free_spins"] += free_n
        draw_reels(grid)
        show_free_spins(free_n)
        return True

    # jackpot
    jp_type = check_jackpot(final)
    if jp_type:
        amount = jp[jp_type]
        state["balance"]   += amount
        state["total_won"] += amount
        if amount > state["best_win"]: state["best_win"] = amount
        draw_reels(grid)
        show_jackpot(jp_type)
        state["streak"] = min(5, state["streak"] + 1)
        return True

    # normal win
    mult = [1,1,1.5,2,3,4][min(state["streak"],5)]
    win, hl = evaluate(final, bet)
    if win > 0:
        final_win = int(win * mult)
        state["balance"]   += final_win
        state["total_won"] += final_win
        if final_win > state["best_win"]: state["best_win"] = final_win
        state["streak"] = min(5, state["streak"] + 1)
        label = "MEGA WIN" if final_win >= bet*40 else ("BIG WIN" if final_win >= bet*15 else "WIN")
        draw_reels(grid, highlights=hl)
        show_win(final_win, label)
        time.sleep(0.5)
    else:
        state["streak"] = 0

    return True

# ─── MAIN LOOP ─────────────────────────────────────────
def main():
    global grid
    grid = [[pick() for _ in range(3)] for _ in range(5)]

    # jackpot ticker thread
    def tick():
        while True:
            jp["grand"] += random.randint(50,150)
            jp["major"] += random.randint(20,80)
            jp["minor"] += random.randint(5,30)
            jp["mini"]  += random.randint(2,10)
            time.sleep(1)
    t = threading.Thread(target=tick, daemon=True)
    t.start()

    draw_reels(grid, msg="أهلاً! اضغط S للدوران 🎰")

    auto_mode = 0

    while True:
        try:
            if auto_mode > 0:
                time.sleep(0.6)
                ok = do_spin()
                if ok: auto_mode -= 1
                else:  auto_mode = 0
                draw_reels(grid, msg=f"🤖 AUTO SPIN – متبقي {auto_mode}" if auto_mode else "")
                continue

            # read key
            try:
                import termios, tty
                fd = sys.stdin.fileno()
                old = termios.tcgetattr(fd)
                try:
                    tty.setraw(fd)
                    key = sys.stdin.read(1).lower()
                finally:
                    termios.tcsetattr(fd, termios.TCSADRAIN, old)
            except Exception:
                key = input("").strip().lower()[:1] or 's'

            if key == 'q':
                console.print("\n[bold yellow]شكراً للعب! 👋[/bold yellow]\n")
                break
            elif key == 's' or key == ' ':
                ok = do_spin()
                if ok: draw_reels(grid)
            elif key == '+' or key == '=':
                state["bet_idx"] = min(len(BETS)-1, state["bet_idx"]+1)
                draw_reels(grid, msg="✅ تم رفع الرهان")
            elif key == '-':
                state["bet_idx"] = max(0, state["bet_idx"]-1)
                draw_reels(grid, msg="✅ تم تخفيض الرهان")
            elif key == 'a':
                auto_mode = 10
                draw_reels(grid, msg="🤖 AUTO SPIN × 10")
            elif key == 'm':
                state["bet_idx"] = len(BETS)-1
                draw_reels(grid, msg="💥 MAX BET!")

        except KeyboardInterrupt:
            console.print("\n[bold yellow]خروج...[/bold yellow]\n")
            break

if __name__ == "__main__":
    main()

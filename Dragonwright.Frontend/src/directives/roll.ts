import type { Directive } from "vue";

export interface RollDirectiveBinding {
  modifier?: number;
  onRoll?: (result: number, rawRoll: number) => void;
}

export const rollDirective: Directive<HTMLElement, RollDirectiveBinding | number | undefined> = {
  mounted(el: HTMLElement, binding) {
    const getValue = (): { modifier: number; onRoll?: (result: number, rawRoll: number) => void } => {
      if (typeof binding.value === 'number') {
        return { modifier: binding.value }
      }
      if (binding.value && typeof binding.value === 'object') {
        return {
          modifier: binding.value.modifier ?? (parseInt(el.innerText, 10) || 0),
          onRoll: binding.value.onRoll
        }
      }
      return { modifier: (parseInt(el.innerText, 10) || 0) }
    }

    el.style.cursor = "pointer";
    el.style.userSelect = "none";

    el.addEventListener("mouseenter", () => {
      el.style.textDecoration = "underline";
    });
    el.addEventListener("mouseleave", () => {
      el.style.textDecoration = "none";
    });
    el.addEventListener("click", () => {
      const { modifier, onRoll } = getValue()
      rollDice(modifier, "normal", onRoll);
    });
    el.addEventListener("contextmenu", e => {
      e.preventDefault();
      const { modifier, onRoll } = getValue()
      const menu = document.createElement("div");
      menu.style.position = "fixed";
      menu.style.top = `${e.clientY}px`;
      menu.style.left = `${e.clientX}px`;
      menu.style.backgroundColor = "#333";
      menu.style.color = "#fff";
      menu.style.padding = "5px";
      menu.style.borderRadius = "5px";
      menu.style.zIndex = "1000";

      const options = ["Normal Roll", "Advantage", "Disadvantage"];
      options.forEach(option => {
        const optionEl = document.createElement("div");
        optionEl.innerText = option;
        optionEl.style.padding = "5px";
        optionEl.style.cursor = "pointer";
        optionEl.addEventListener("click", () => {
          document.body.removeChild(menu);
          if (option === "Normal Roll") {
            rollDice(modifier, "normal", onRoll);
          } else if (option === "Advantage") {
            rollDice(modifier, "advantage", onRoll);
          } else if (option === "Disadvantage") {
            rollDice(modifier, "disadvantage", onRoll);
          }
        });
        optionEl.addEventListener("mouseenter", () => {
          optionEl.style.backgroundColor = "#555";
        });
        optionEl.addEventListener("mouseleave", () => {
          optionEl.style.backgroundColor = "transparent";
        });
        menu.appendChild(optionEl);
      });

      document.body.appendChild(menu);

      const removeMenu = () => {
        if (document.body.contains(menu)) {
          document.body.removeChild(menu);
        }
      };

      document.addEventListener("click", removeMenu, { once: true });
    });
  }
};

function rollDice(
  mod: number,
  mode: "normal" | "advantage" | "disadvantage",
  onRoll?: (result: number, rawRoll: number) => void
) {
  let rolls: string[] = [];
  let rollResult: number | undefined;
  for (let i = 0; i < (mode === "normal" ? 1 : 2); i++) {
    const roll = Math.floor(Math.random() * 20) + 1;
    rolls.push(`${roll} + ${mod} = ${roll + mod}`);
    if (mode === "normal") {
      rollResult = roll;
    } else if (mode === "advantage") {
      if (rollResult === undefined || roll > rollResult) {
        rollResult = roll;
      }
    } else if (mode === "disadvantage") {
      if (rollResult === undefined || roll < rollResult) {
        rollResult = roll;
      }
    }
  }

  const total = rollResult! + mod;
  alert(`You rolled: ${rolls.join(", ")}\nResult: ${total}`);

  if (onRoll) {
    onRoll(total, rollResult!);
  }
}

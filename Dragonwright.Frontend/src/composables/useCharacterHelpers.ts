import type {Ref} from "vue";
import {skillLabels} from "@/content/enums";

export type ProfLevel = "none" | "half" | "prof" | "expert";

export const skillToAbility: Record<number, number> = {
  0: 1, 1: 4, 2: 3, 3: 0, 4: 5, 5: 3, 6: 4, 7: 5,
  8: 3, 9: 4, 10: 3, 11: 4, 12: 5, 13: 5, 14: 3, 15: 1, 16: 1, 17: 4,
};

export const abilityAbbrev: Record<number, string> = {
  0: "STR", 1: "DEX", 2: "CON", 3: "INT", 4: "WIS", 5: "CHA",
};

export const skillList = Object.entries(skillLabels).map(([k, v]) => ({id: Number(k), label: v}));

export function useCharacterHelpers(character: Ref<any>) {
  function signed(n: number): string {
    return n >= 0 ? `+${n}` : `${n}`;
  }

  function getAbilityScore(ability: number): number {
    const ab = character.value?.abilities?.find((a: any) => a.ability === ability);
    return ab?.score ?? 10;
  }

  function getAbilityMod(score: number): string {
    const mod = Math.floor((score - 10) / 2);
    return mod >= 0 ? `+${mod}` : `${mod}`;
  }

  function getAbilityModRaw(ability: number): number {
    const score = getAbilityScore(ability);
    return Math.floor((score - 10) / 2);
  }

  function getSavingThrow(ability: number): string {
    const ab = character.value?.abilities?.find((a: any) => a.ability === ability);
    if (!ab) return "+0";
    const mod = ab.savingThrow;
    return mod >= 0 ? `+${mod}` : `${mod}`;
  }

  function getSaveProficiencyLevel(ability: number): ProfLevel {
    const p = character.value?.abilities?.find((a: any) => a.ability === ability)?.savingThrowProficiency ?? 0;
    if (p >= 3) return "expert";
    if (p === 2) return "prof";
    if (p === 1) return "half";
    return "none";
  }

  function getSkillBonus(skill: number): string {
    const sk = character.value?.skills?.find((s: any) => s.skill === skill);
    if (!sk) return "+0";
    const mod = sk.total;
    return mod >= 0 ? `+${mod}` : `${mod}`;
  }

  function getSkillProficiencyLevel(skill: number): ProfLevel {
    const p = character.value?.skills?.find((s: any) => s.skill === skill)?.proficiency ?? 0;
    if (p >= 3) return "expert";
    if (p === 2) return "prof";
    if (p === 1) return "half";
    return "none";
  }

  function getSkillAbility(skillId: number): string {
    const abilityId = skillToAbility[skillId] ?? 0;
    return abilityAbbrev[abilityId] ?? "";
  }

  return {
    signed,
    getAbilityScore,
    getAbilityMod,
    getAbilityModRaw,
    getSavingThrow,
    getSaveProficiencyLevel,
    getSkillBonus,
    getSkillProficiencyLevel,
    getSkillAbility,
  };
}

import type { SourceType } from '@/api'
import {
  getSpells, getItems, getClasses, getRaces, getFeats, getBackgrounds, getLanguages,
  deleteSpellsId, deleteItemsId, deleteClassesId, deleteRacesId, deleteFeatsId, deleteBackgroundsId, deleteLanguagesId,
  getSpellsId, postSpells, putSpellsId,
  getItemsId, postItems, putItemsId,
  getClassesId, postClasses, putClassesId,
  getRacesId, postRaces, putRacesId,
  getFeatsId, postFeats, putFeatsId,
  getBackgroundsId, postBackgrounds, putBackgroundsId,
  getLanguagesId, postLanguages, putLanguagesId,
} from '@/api'

export interface ContentTypeDefinition {
  key: string
  label: string
  singular: string
  icon: string
  description: string
  hasSourceFilter: boolean
  teamOnly: boolean
  fetchFn: (params: ContentListParams) => Promise<any>
  deleteFn: (id: string) => Promise<any>
  getFn?: (id: string) => Promise<any>
  createFn?: (data: any) => Promise<any>
  updateFn?: (id: string, data: any) => Promise<any>
}

export interface ContentListParams {
  page?: number | string
  pageSize?: number | string
  search?: string
  source?: SourceType
}

export const contentTypes: ContentTypeDefinition[] = [
  {
    key: 'spells',
    label: 'Spells',
    singular: 'Spell',
    icon: 'fas fa-hat-wizard',
    description: 'Magical effects wielded by casters across all schools of magic.',
    hasSourceFilter: true,
    teamOnly: false,
    fetchFn: (p) => getSpells(p),
    deleteFn: (id) => deleteSpellsId(id),
    getFn: (id) => getSpellsId(id),
    createFn: (data) => postSpells(data),
    updateFn: (id, data) => putSpellsId(id, data),
  },
  {
    key: 'items',
    label: 'Items',
    singular: 'Item',
    icon: 'fas fa-ring',
    description: 'Weapons, armor, potions, and wondrous objects.',
    hasSourceFilter: true,
    teamOnly: false,
    fetchFn: (p) => getItems(p),
    deleteFn: (id) => deleteItemsId(id),
    getFn: (id) => getItemsId(id),
    createFn: (data) => postItems(data),
    updateFn: (id, data) => putItemsId(id, data),
  },
  {
    key: 'classes',
    label: 'Classes',
    singular: 'Class',
    icon: 'fas fa-shield-halved',
    description: 'Adventurer archetypes that define a character\u2019s abilities.',
    hasSourceFilter: true,
    teamOnly: false,
    fetchFn: (p) => getClasses(p),
    deleteFn: (id) => deleteClassesId(id),
    getFn: (id) => getClassesId(id),
    createFn: (data) => postClasses(data),
    updateFn: (id, data) => putClassesId(id, data),
  },
  {
    key: 'races',
    label: 'Races',
    singular: 'Race',
    icon: 'fas fa-dragon',
    description: 'Playable species and lineages with unique traits.',
    hasSourceFilter: true,
    teamOnly: false,
    fetchFn: (p) => getRaces(p),
    deleteFn: (id) => deleteRacesId(id),
    getFn: (id) => getRacesId(id),
    createFn: (data) => postRaces(data),
    updateFn: (id, data) => putRacesId(id, data),
  },
  {
    key: 'feats',
    label: 'Feats',
    singular: 'Feat',
    icon: 'fas fa-star',
    description: 'Special talents and abilities that grant unique benefits.',
    hasSourceFilter: true,
    teamOnly: false,
    fetchFn: (p) => getFeats(p),
    deleteFn: (id) => deleteFeatsId(id),
    getFn: (id) => getFeatsId(id),
    createFn: (data) => postFeats(data),
    updateFn: (id, data) => putFeatsId(id, data),
  },
  {
    key: 'backgrounds',
    label: 'Backgrounds',
    singular: 'Background',
    icon: 'fas fa-scroll',
    description: 'A character\u2019s history, skills, and origin story.',
    hasSourceFilter: true,
    teamOnly: false,
    fetchFn: (p) => getBackgrounds(p),
    deleteFn: (id) => deleteBackgroundsId(id),
    getFn: (id) => getBackgroundsId(id),
    createFn: (data) => postBackgrounds(data),
    updateFn: (id, data) => putBackgroundsId(id, data),
  },
  {
    key: 'languages',
    label: 'Languages',
    singular: 'Language',
    icon: 'fas fa-language',
    description: 'Spoken and written tongues of the realm.',
    hasSourceFilter: false,
    teamOnly: true,
    fetchFn: (p) => getLanguages(p),
    deleteFn: (id) => deleteLanguagesId(id),
    getFn: (id) => getLanguagesId(id),
    createFn: (data) => postLanguages(data),
    updateFn: (id, data) => putLanguagesId(id, data),
  },
]

export function getContentType(key: string): ContentTypeDefinition | undefined {
  return contentTypes.find((t) => t.key === key)
}

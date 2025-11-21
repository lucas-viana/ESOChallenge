/**
 * Response from Fortnite API /v2/news endpoint
 */
export interface NewsApiResponse {
  status: number
  data: NewsData
}

/**
 * News data containing information for different game modes
 */
export interface NewsData {
  br: NewsGameMode
  stw: NewsGameMode
  creative: NewsGameMode
}

/**
 * News information for a specific game mode (BR, STW, or Creative)
 */
export interface NewsGameMode {
  hash: string
  date: string
  image: string | null
  motds: Motd[]
  messages: NewsMessage[]
}

/**
 * Message of the Day (MOTD)
 */
export interface Motd {
  id: string
  title: string
  tabTitle: string
  body: string
  image: string | null
  tileImage: string | null
  sortingPriority: number
  hidden: boolean
  websiteUrl: string | null
  videoString: string | null
  videoId: string | null
}

/**
 * News message
 */
export interface NewsMessage {
  title: string
  body: string
  image: string | null
  adspace: string | null
}

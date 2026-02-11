import { ref } from 'vue'

export interface FenceEvent {
  id: string | number
  deviceName: string
  eventType: number
  date: string
  time: string
  imageUrl?: string
}

export function useFenceModal() {
  const showFenceModal = ref(false)
  const selectedFenceEvent = ref<FenceEvent | null>(null)

  const showFenceEventDetail = (event: FenceEvent) => {
    selectedFenceEvent.value = event
    showFenceModal.value = true
  }

  const closeFenceModal = () => {
    showFenceModal.value = false
    selectedFenceEvent.value = null
  }

  return {
    showFenceModal,
    selectedFenceEvent,
    showFenceEventDetail,
    closeFenceModal
  }
}

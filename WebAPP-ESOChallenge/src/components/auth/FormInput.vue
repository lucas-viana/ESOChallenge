<!-- 
  FormInput Component
  Componente reutilizável para inputs de formulário
  Segue Single Responsibility Principle
-->

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  modelValue: string
  type?: 'text' | 'email' | 'password'
  label: string
  placeholder?: string
  error?: string
  required?: boolean
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  type: 'text',
  placeholder: '',
  error: '',
  required: false,
  disabled: false
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const hasError = computed(() => !!props.error)

const inputClasses = computed(() => ({
  'form-input': true,
  'form-input--error': hasError.value,
  'form-input--disabled': props.disabled
}))

function handleInput(event: Event) {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.value)
}
</script>

<template>
  <div class="form-group">
    <label :for="label" class="form-label">
      {{ label }}
      <span v-if="required" class="form-label__required">*</span>
    </label>
    
    <input
      :id="label"
      :type="type"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :class="inputClasses"
      :aria-invalid="hasError"
      :aria-describedby="hasError ? `${label}-error` : undefined"
      @input="handleInput"
    />
    
    <span
      v-if="hasError"
      :id="`${label}-error`"
      class="form-error"
      role="alert"
    >
      {{ error }}
    </span>
  </div>
</template>

<style scoped>
.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 20px;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #d1d5db;
  letter-spacing: 0.025em;
}

.form-label__required {
  color: #ef4444;
  margin-left: 4px;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  font-size: 1rem;
  background: rgba(255, 255, 255, 0.05);
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: #ffffff;
  transition: all 0.3s ease;
  outline: none;
}

.form-input::placeholder {
  color: #6b7280;
}

.form-input:focus {
  border-color: #667eea;
  background: rgba(255, 255, 255, 0.08);
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-input--error {
  border-color: #ef4444;
}

.form-input--error:focus {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.form-input--disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.form-error {
  font-size: 0.875rem;
  color: #ef4444;
  display: flex;
  align-items: center;
  gap: 4px;
}

.form-error::before {
  content: '⚠';
}
</style>

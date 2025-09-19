import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ModalComponent } from '../modal/modal.component';

@Component({
  selector: 'app-address-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ModalComponent],
  templateUrl: './adress-form.component.html',
  styleUrls: ['./adress-form.component.css']
})
export class AddressFormComponent implements OnInit, OnChanges {
  @Input() addresses: any[] = [];
  @Output() addressesChange = new EventEmitter<any[]>();
  
  internalAddresses: any[] = [];
  
  isModalOpen = false;
  addressForm: FormGroup;

  constructor(private fb: FormBuilder) {
    this.addressForm = this.fb.group({
      street: ['', Validators.required],
      number: ['', Validators.required],
      complement: [''],
      neighborhood: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipcode: ['', [Validators.required, Validators.pattern(/^\d{8}$/)]]
    });
  }

  ngOnInit() {
    this.internalAddresses = [...this.addresses];
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['addresses'] && changes['addresses'].currentValue) {
      this.internalAddresses = [...changes['addresses'].currentValue];
    }
  }

  openModal() {
    this.isModalOpen = true;
    this.addressForm.reset();
  }

  closeModal() {
    this.isModalOpen = false;
    this.addressForm.reset();
  }

  onSubmit() {
    if (this.addressForm.valid) {
      const newAddress = this.addressForm.value;
      this.internalAddresses.push(newAddress);
      this.addressesChange.emit([...this.internalAddresses]);
      this.closeModal();
    }
  }

  removeAddress(index: number) {
    if (this.internalAddresses.length > 1) {
      this.internalAddresses.splice(index, 1);
      this.addressesChange.emit([...this.internalAddresses]);
    }
  }

  editAddress(index: number) {
    const address = this.internalAddresses[index];
    this.addressForm.patchValue(address);
    this.isModalOpen = true;
    
    // Remove address from list temporarily for editing
    this.internalAddresses.splice(index, 1);
    this.addressesChange.emit([...this.internalAddresses]);
  }
}

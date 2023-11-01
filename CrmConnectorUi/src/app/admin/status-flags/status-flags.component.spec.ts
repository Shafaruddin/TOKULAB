import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusFlagsComponent } from './status-flags.component';

describe('StatusFlagsComponent', () => {
  let component: StatusFlagsComponent;
  let fixture: ComponentFixture<StatusFlagsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StatusFlagsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(StatusFlagsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
